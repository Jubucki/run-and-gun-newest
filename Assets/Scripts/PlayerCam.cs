using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
 public float sensX;
 public float sensY;

 public Transform orientation;

 private float xRotation;
 private float yRotation;

 private void Start()
 {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
 }

 private void Update()
    {
         // get mouse input
         float mouseX = Mouse.current.delta.x.ReadValue() * Time.deltaTime * sensX;
         float mouseY = Mouse.current.delta.y.ReadValue() * Time.deltaTime * sensY;

         yRotation += mouseX;

         xRotation -= mouseY;
         xRotation = Mathf.Clamp(xRotation, -90f, 90);
         
         
         //rotate cam and orientation
         transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
         orientation.rotation = Quaternion.Euler(0, yRotation, 0);


         //https://www.youtube.com/watch?v=f473C43s8nE
    }
 
}
