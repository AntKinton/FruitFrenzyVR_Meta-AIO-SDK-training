using Oculus.Interaction;
using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] public char katanaId;

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleCollision(other);
    }

    // He unificado la lógica de colisión para que sea más limpia
    private void HandleCollision(Collider other)
    {
        if (GameManager.Instance.State != GameState.Play) return;

        // 1. Si golpeamos el pez entero
        if (other.gameObject.layer == LayerMask.NameToLayer("WholeFruit"))
        {
            var fruitScript = other.gameObject.GetComponent<Fruit>();
            if (fruitScript != null && !fruitScript.hasBeenSliced) 
            {
                SliceFruit(fruitScript);
            }
        }
        // 2. Si golpeamos un trozo (¡Fileteado!)
        else if (other.gameObject.layer == LayerMask.NameToLayer("HalfFruit"))
        {
            var halfScript = other.gameObject.GetComponent<HalfFruitRuntime>();
            if (halfScript != null && !halfScript.hasBeenSliced)
            {
                SliceHalfFruit(halfScript);
            }
        }
    }

    // Corte original
    public void SliceFruit(Fruit fruitScript)
    {
        float amplitude = fruitScript.isBomba ? 0.8f : 0.4f;
        VibrateController(amplitude, fruitScript.isBomba);

        Vector3 sliceNormal = transform.right;
        Vector3 slicePoint = fruitScript.transform.position;
        fruitScript.slicePlane = new Plane(sliceNormal, slicePoint);
        
        fruitScript.Slice();
    }

    // NUEVO: Corte de los filetes
    public void SliceHalfFruit(HalfFruitRuntime halfScript)
    {
        // Vibración más suave porque es un trozo más pequeño
        VibrateController(0.2f, false); 

        Vector3 sliceNormal = transform.right;
        Vector3 slicePoint = halfScript.transform.position;
        Plane slicePlane = new Plane(sliceNormal, slicePoint);
        
        halfScript.Slice(slicePlane);
    }

    // He actualizado esta función para que reciba directamente si es bomba o no, 
    // así podemos usarla tanto con Fruit como con HalfFruitRuntime
    public void VibrateController(float amplitude, bool isBomba)
    {
        if (katanaId == 'r')
        {
            OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.RTouch);
            if (isBomba) OVRInput.SetControllerVibration(1, amplitude * 0.5f, OVRInput.Controller.LTouch);
            
            StartCoroutine(VibrationTime(isBomba ? 0.6f : 0.3f, isBomba));
        }
        else if (katanaId == 'l')
        {
            OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.LTouch);
            if (isBomba) OVRInput.SetControllerVibration(1, amplitude * 0.5f, OVRInput.Controller.RTouch); 
            
            StartCoroutine(VibrationTime(isBomba ? 0.6f : 0.3f, isBomba));
        }
    }

    private IEnumerator VibrationTime(float seconds, bool isBomb)
    {
        yield return new WaitForSeconds(seconds);
        if (katanaId == 'r')
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            if (isBomb) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }
        else if (katanaId == 'l')
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            if (isBomb) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }
}
