using UnityEngine;

public class DynamicCam : MonoBehaviour
{
    public GameObject vCam2;
    
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CamTrigger":
                vCam2.SetActive(true);
                break;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CamTrigger":
                vCam2.SetActive(false);
                break;
        }
    }
}