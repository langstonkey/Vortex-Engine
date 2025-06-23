using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Rendering.CameraUI;
using static UnityEngine.InputSystem.LowLevel.InputEventTrace;

public class Portal
{
    public string path;

    PortalConnection portalConnection;

    public bool isActive;

    public void Initialize()
    {
        portalConnection = new PortalConnection(path);
    }

    public void ReadyCommand()
    {
        byte[] input = new byte[0x21];
        input[1] = Convert.ToByte('R');

        byte[] output;

        do
        {
            portalConnection.Write(input);

            output = portalConnection.Read();
        } while (HandleResponse(output) != Convert.ToByte('R'));

        Debug.Log("Portal is Ready - " + path);
        Debug.Log(output[1] + " - " + output[2]);
    }

    public void ActivateCommand()
    {
        byte[] input = new byte[0x21];
        input[1] = Convert.ToByte('A');
        input[2] = 0x01;

        byte[] output;

        do
        {
            portalConnection.Write(input);

            output = portalConnection.Read();
        } while (HandleResponse(output) != Convert.ToByte('A') && output[1] == 1);

        Debug.Log("Portal is Active - " + path);
        Debug.Log(output[1] + " - " + output[2]);
    }

    public void StatusCommand(bool verbose = false)
    {
        byte[] input = new byte[0x21];
        input[1] = Convert.ToByte('S');

        byte[] output;

        do
        {
            portalConnection.Write(input);

            output = portalConnection.Read();

            // we do not handle status here - in HandleResponse//
        } while (HandleResponse(output, verbose) != Convert.ToByte('S'));
    }

    public void ColorCommand(byte r, byte g, byte b)
    {
        byte[] input = new byte[0x21];

        input[1] = Convert.ToByte('C');
        input[2] = r;
        input[3] = g;
        input[4] = b;

        portalConnection.Write(input);
    }

    public void Query(byte characterIndex, byte block, bool verbose)
    {
        byte[] input = new byte[0x21];

        input[1] = Convert.ToByte('Q');
        input[2] = characterIndex;
        input[3] = block;
        byte[] output;

        do
        {
            portalConnection.Write(input);
            output = portalConnection.Read();
        }
        while (HandleQuery(output, verbose) != Convert.ToByte('Q'));
    }
    public void Write(byte characterIndex, byte block, byte[] data, bool verbose)
    {
        if (data.Length > 16)
        {
            Debug.LogWarning("Too many bytes in write attempt");
            return;
        }

        byte[] input = new byte[0x21];

        input[1] = Convert.ToByte('W');
        input[2] = characterIndex;
        input[3] = block;
            
        for (int i = 0; i < 16; i++)
        {
            input[i + 4] = data[i];
        }

        byte[] output;

        do
        {
            portalConnection.Write(input);
            output = portalConnection.Read();
        }
        while (HandleWrite(output, verbose) != Convert.ToByte('W'));
    }

    public byte HandleResponse(byte[] input, bool verbose = false)
    {
        if(input.Length != 0x20)
        {
            throw new Exception();
        }

        if (input[0] == Convert.ToByte('S'))
        {
            if (verbose) Debug.Log(BitConverter.ToString(input));

            byte[] figureStatusData = input.Skip(1).Take(4).ToArray();
            int figureStatusInt = BitConverter.ToInt32(figureStatusData);

            var figureStatus = new BitArray(new int[] { figureStatusInt });

            for (int i = figureStatus.Length - 1; i >= 1; i-=2)
            {
                bool present = figureStatus.Get(i - 1);
                bool changed = figureStatus.Get(i);

                if (changed)
                {
                    Debug.Log("Changed");
                }
            }
        }

        return input[0];
    }

    public byte HandleQuery(byte[] input, bool verbose = false)
    {
        if (input.Length != 0x20)
        {
            throw new Exception();
        }

        if (verbose)
        {
            if (input[6] != 1) Debug.Log(BitConverter.ToString(input));
        }

        return input[0];
    }

    public byte HandleWrite(byte[] input, bool verbose = false)
    {
        if (input.Length != 0x20)
        {
            throw new Exception();
        }

        if (verbose)
        {
            if (input[6] != 1) Debug.Log(BitConverter.ToString(input));
        }

        return input[0];
    }
}
