using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEditor.Experimental.GraphView;

public class PortalManager : MonoBehaviour
{
    public static Dictionary<string, Portal> portals = new Dictionary<string, Portal>();
    [Min(1)] public float pollingRate = 0;

    IEnumerator Start()
    {
        StartCoroutine("CreatePortalConnections");

        //StartCoroutine("ColorMe");

        foreach (var portal in portals.Values)
        {
            portal.ReadyCommand();
        }

        foreach (var portal in portals.Values)
        {
            portal.ActivateCommand();
        }

        while(true)
        {
            foreach (var portal in portals.Values)
            {
                portal.StatusCommand();
            }

            yield return new WaitForSeconds(1f / 50f);
        }
        
    }

    IEnumerator CreatePortalConnections()
    {
        hidapi.hid_init();

        IntPtr firstDevice = hidapi.hid_enumerate(0x1430, 0x0150);
        IntPtr currDevice = firstDevice;

        while (currDevice != IntPtr.Zero)
        {
            hidDeviceInfo deviceInfo = Marshal.PtrToStructure<hidDeviceInfo>(currDevice);

            if (deviceInfo.vendor_id == 0x1430 && deviceInfo.product_id == 0x0150)
            {
                Debug.Log("PORTAL FOUND ");

                if (!portals.ContainsKey(deviceInfo.path))
                {
                    portals.Add(deviceInfo.path, new Portal());

                    portals[deviceInfo.path].path = deviceInfo.path;

                    portals[deviceInfo.path].Initialize();

                    portals[deviceInfo.path].isActive = true;
                }
            }

            currDevice = deviceInfo.next;
        }

        hidapi.hid_free_enumeration(firstDevice);

        yield return new WaitForSeconds(2f);

        //StartCoroutine("CreatePortalConnections");
    }

    IEnumerator ColorMe()
    {
        foreach (var portalEntry in portals)
        {

            Portal portal = portalEntry.Value;

            // Set random colors for demonstration
            byte red = (byte)UnityEngine.Random.Range(0, 256);
            byte green = (byte)UnityEngine.Random.Range(0, 256);
            byte blue = (byte)UnityEngine.Random.Range(0, 256);

            // Send the color command to change the portal color
            portal.ColorCommand(red, green, blue);
        }

        yield return new WaitForSeconds(1f/20f);

        StartCoroutine("ColorMe");
    }

    [Button]
    public void PortalStatus()
    {
        foreach (var portalEntry in portals)
        {
            Portal portal = portalEntry.Value;
            portal.StatusCommand(true);
        }
    }

    [SerializeField] byte characterIndex = 0;
    [SerializeField] byte[] data;
    [Button]
    public void PortalQuery()
    {
        if (queryInProgress)
        {
            Debug.LogWarning("Query in progress, repeated requests are ignored");
        }
        else
        {
            StartCoroutine(PortalQueryAllBytes());
        }
    }

    [Button]
    public void PortalWrite()
    {
        if (writeInProgress)
        {
            Debug.LogWarning("Write in progress, repeated requests are ignored");
        }
        else
        {
            StartCoroutine(PortalWriteAllBytes());
        }
    }

    bool queryInProgress = false;
    IEnumerator PortalQueryAllBytes()
    {
        queryInProgress = true;
        foreach (var portalEntry in portals)
        {
            Portal portal = portalEntry.Value;

            for (int i = 0; i < 64; i++)
            {
                portal.Query(characterIndex, (byte)i, true);

                yield return new WaitForSeconds(1f / pollingRate);
            }
        }
        queryInProgress = false;
    }
    bool writeInProgress = false;
    IEnumerator PortalWriteAllBytes()
    {
        writeInProgress = true;
        foreach (var portalEntry in portals)
        {
            Portal portal = portalEntry.Value;

            for (int i = 0; i < 64; i++)
            {
                portal.Write(characterIndex, (byte)i, data, true);

                yield return new WaitForSeconds(1f / pollingRate);
                yield return new WaitForSeconds(1f / pollingRate);
            }
        }
        writeInProgress = false;
    }
}
