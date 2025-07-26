using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ServiceLocator
{
    private static List<IService> _currentServices = new List<IService>();

    public static void AddService<T>(T service) where T : IService
    {
        if(service != null)
        {
            _currentServices.Add(service);
        }
    }

    public static void RemoveService<T>(T service) where T : IService
    {
        if (service != null)
        {
            _currentServices.Remove(service);
        }
    }

    public static async UniTask<T> GetService<T>() where T : IService
    {
        foreach (IService currentService in _currentServices)
        {
            if (currentService is T)
            {
                return (T)currentService;
            }
        }

        Debug.LogWarning("No service " + typeof(T) + " was found, waiting for one...");
        int lastServicesAmount = _currentServices.Count;
        await UniTask.WaitUntil(() => _currentServices.Count != lastServicesAmount);
        return await GetService<T>();
    }
}
