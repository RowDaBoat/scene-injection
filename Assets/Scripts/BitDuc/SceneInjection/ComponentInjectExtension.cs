using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace BitDuc.SceneInjection
{
    public static class ComponentInjectExtension
    {
        static GameObject staticObject;

        public static void Inject(this Component target) =>
            Resolve(target, Injections(target));

        static void Resolve(Component target, IEnumerable<(FieldInfo field, Component dependency)> valueTuples)
        {
            foreach (var injection in valueTuples)
            {
                ValidateDependency(target, injection);
                injection.field.SetValue(target, injection.dependency);
            }
        }

        static void ValidateDependency(Component target, (FieldInfo, Component) injection)
        {
            var (field, dependency) = injection;

            if (dependency != null)
                return;
            
            if (!HasAnyAttribute<OptionalAttribute>(field))
                Debug.LogError(FailMessage(target, field), target);
        }

        static string FailMessage(Component target, FieldInfo field) =>
            $"Dependency {field.Name} of type {field.FieldType.Name} not found, in requesting component" +
            $" {target.GetType().Name} on GameObject {target.name}.";

        static IEnumerable<(FieldInfo field, Component dependency)> Injections(Component target) =>
            AllFieldsFrom(target)
                .Select(field => DependencyFrom(target, field))
                .Where(injection => injection != default);

        static (FieldInfo, Component) DependencyFrom(Component target, FieldInfo field) =>
            HasAnyAttribute<FromSelfAttribute>(field) ? ComponentInSelf(target, field) :
            HasAnyAttribute<FromChildAttribute>(field) ? ComponentInChildren(target, field) :
            HasAnyAttribute<FromRootAttribute>(field) ? ComponentInRoots(field) :
            HasAnyAttribute<FromParentAttribute>(field) ? ComponentInParent(target, field) :
            HasAnyAttribute<FromSiblingAttribute>(field) ? ComponentInSibling(target, field) :
            default;

        static (FieldInfo, Component) ComponentInSelf(Component target, FieldInfo field) =>
            (field, target.GetComponent(field.FieldType));

        static (FieldInfo, Component) ComponentInChildren(Component target, FieldInfo field) =>
            (field, target.GetComponentInChildren(field.FieldType, true));

        static (FieldInfo, Component) ComponentInRoots(FieldInfo field) =>
            (field, GetComponentInRoots(field.FieldType));

        static (FieldInfo, Component) ComponentInParent(Component target, FieldInfo field) =>
            (field, target.GetComponentInParent(field.FieldType));

        static (FieldInfo, Component) ComponentInSibling(Component target, FieldInfo field) =>
            (field, GetComponentInSiblings(target, field.FieldType));

        static Component GetComponentInSiblings(Component target, Type type) =>
            target.transform.parent.Cast<Transform>()
                .SelectMany(child => child.GetComponents(type))
                .FirstOrDefault();

        static bool HasAnyAttribute<T>(FieldInfo field) =>
            field.GetCustomAttributes(typeof(T), true).Any();

        static FieldInfo[] AllFieldsFrom(Component target) =>
            target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        static Component GetComponentInRoots(Type type) =>
            AllObjects()
                .Select(gameObject => gameObject.GetComponent(type))
                .First(component => component != null);

        static IEnumerable<GameObject> AllObjects() =>
            ActiveSceneObjects.Concat(DontDestroyOnLoadObjects);

        static GameObject[] ActiveSceneObjects =>
            SceneManager.GetActiveScene().GetRootGameObjects();

        static GameObject[] DontDestroyOnLoadObjects =>
            StaticObject().scene.GetRootGameObjects();

        static GameObject StaticObject()
        {
            if (staticObject != null)
                return staticObject;

            staticObject = new GameObject();
            Object.DontDestroyOnLoad(staticObject);
            return staticObject;
        }
    }
}
