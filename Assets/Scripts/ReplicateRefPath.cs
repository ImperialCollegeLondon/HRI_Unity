using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class replicateRefPath : MonoBehaviour
{
    public int _nrings;
    private List<GameObject> path_;

    // Use this for initialization
    void Start()
    {
        path_ = new List<GameObject>();
        Vector3 tip_position = GameObject.Find("needle").transform.position;
        Quaternion tip_orientation = GameObject.Find("needle").transform.localRotation;
        for (int i = 0; i < _nrings; i++)
        {
            path_.Add((GameObject)Instantiate(Resources.Load("RefPath"), new Vector3(0, 0, i * 2.0f) + tip_position, tip_orientation));
            path_[i].name = "Ring " + i;
        }
        print("List size " + path_.Count);
    }

    // Update is called once per frame
    void Update()
    {
        // anelloPipe2.setPosition(p);
    }

}
