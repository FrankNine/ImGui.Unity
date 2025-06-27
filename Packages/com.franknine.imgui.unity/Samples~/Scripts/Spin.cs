using UnityEngine;

public class Spin : MonoBehaviour
{
    private void Update() 
        => gameObject.transform.Rotate(Vector3.up, 3);
}
