using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOXROTATION : MonoBehaviour{
    float smooth = 5.0f;
    float tiltAngle = 60.0f;
    float tiltAroundZ;
    float tiltAroundX;
    float tiltAroundY;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Smoothly tilts a transform towards a target rotation.
        tiltAroundZ += Random.Range(-0.125f,0.25f) * tiltAngle;
        tiltAroundX += Random.Range(-0.125f,0.25f) * tiltAngle;
        tiltAroundY += Random.Range(-0.125f,0.25f) * tiltAngle;

        // Rotate the cube by converting the angles into a quaternion.
        Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, tiltAroundZ);

        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);
    }
}
