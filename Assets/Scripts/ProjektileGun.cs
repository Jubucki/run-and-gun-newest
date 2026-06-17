using System;  
using System.Collections;
using UnityEngine;  
using TMPro;
using Random = UnityEngine.Random;

//using Random = System.Random;  

public class Projectile : MonoBehaviour
{
    //bullet 
    public GameObject bullet;
    public float damage = 100f;

    //bullet force
    public float shootForce, upwardForce;

    //Gun stats
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;

    //Recoil
    public Rigidbody playerRb;
    public float recoilForce;

    //bools
    bool shooting, readyToShoot, reloading;

    //Reference
    public Camera fpsCam;
    public Transform attackPoint;

    //Graphics
    public GameObject muzzleFlash;
    public float flashDuration = 0.05f;
    public TextMeshProUGUI ammunitionDisplay;

    //bug fixing :D
    public bool allowInvoke = true;

    [Header("Audio")]
    public AudioClip shootSound;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        //make sure magazine is full
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        MyInput();

        //Set ammo display, if it exists :D
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
    }
    private void MyInput()
    {
        //Check if allowed to hold down button and take corresponding input
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        //Reloading 
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        //Reload automatically when trying to shoot without ammo
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        //Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            //Set bullets shot to 0
            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Play shooting sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Get ray through the center of the camera (exactly matching the crosshair)
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // Add spread to the ray direction  
        Vector3 rayDirection = ray.direction + fpsCam.transform.right * x + fpsCam.transform.up * y;
        rayDirection.Normalize();

        RaycastHit hit;
        int layerMask = ~(1 << 13); // Ignore Player layer (13)

        // Perform raycast directly from camera to find hits
        float sphereRadius = 0.1f; // Radius anpassen
        if (Physics.SphereCast(ray.origin, sphereRadius, rayDirection, out hit, 500f, layerMask))
        {
            Debug.Log($"Raycast hit: {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            // Search for EnemyHealth component in parent or children
            EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = hit.transform.root.GetComponentInChildren<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                Debug.Log($"EnemyHealth found on {enemyHealth.gameObject.name}, dealing {damage} damage via Camera Raycast.");
                enemyHealth.TakeDamage(damage);
            }
        }

        //Instantiate muzzle flash, if you have one
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            CancelInvoke("HideFlash");
            Invoke("HideFlash", flashDuration);
            
            
        }
        else
        {
            Debug.Log("muzzleFlash is NULL!");
        }

        bulletsLeft--;
        bulletsShot++;

        //Invoke resetShot function (if not already invoked), with your timeBetweenShooting
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

            //Add recoil to player (should only be called once)
            playerRb.AddForce(-rayDirection * recoilForce, ForceMode.Impulse);
        }

        //if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }
    private void ResetShot()
    {
        //Allow shooting and invoking again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime); //Invoke ReloadFinished function with your reloadTime as delay
    }
    private void ReloadFinished()
    {
        //Fill magazine
        bulletsLeft = magazineSize;
        reloading = false;
    }
    
    private void HideFlash()
    {
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }
    
    IEnumerator FlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        muzzleFlash.SetActive(false);
    }
}
