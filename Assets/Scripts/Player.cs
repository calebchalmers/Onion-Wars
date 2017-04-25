﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [Header("References")]
    public Transform neck;
    public Transform head;
    public Camera cam;

    private UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration vignette;
    private UnityStandardAssets.ImageEffects.SunShafts sunShafts;
    //private UnityStandardAssets.ImageEffects.DepthOfField depthOfField;

    [HideInInspector]
    public float rotationX;
    [HideInInspector, SyncVar(hook = "UpdateScore")]
    public int score = 0;
    [HideInInspector, SyncVar(hook = "UpdateUsername")]
    public string username = "Player";

    private float defaultFov;
    private float defaultVignette;
    private float defaultBlur;

    void Start()
    {
        GameManager.instance.AddPlayer(this);
    }

    public override void OnStartLocalPlayer()
    {
        //CmdSetUsername(System.Environment.UserName);
        CmdSetUsername(PlayerPrefs.GetString("username", username));

        cam.gameObject.SetActive(true);

        vignette = cam.GetComponent<UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration>();
        sunShafts = cam.GetComponent<UnityStandardAssets.ImageEffects.SunShafts>();
        //depthOfField = cam.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();

        rotationX = transform.eulerAngles.x;
        defaultFov = cam.fieldOfView;
        defaultVignette = vignette.intensity;
        defaultBlur = vignette.blur;

        sunShafts.sunTransform = GameObject.Find("Sun").transform;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (CursorHelper.CursorLocked)
        {
            // Mouse look
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            float sensitivity = GameManager.instance.mouseSensitivity;

            transform.Rotate(0f, mouseX * sensitivity, 0f);

            rotationX = Mathf.Clamp(rotationX - mouseY * sensitivity, -90f, 90f);
            neck.localEulerAngles = new Vector3(rotationX * 0.65f, 0f, 0f);
            head.localEulerAngles = new Vector3(rotationX * 0.35f, 0f, 0f);

            // Zoom
            float newFOV = defaultFov;
            float newVignette = defaultVignette;
            float newBlur = defaultBlur;
            if (Input.GetKey(KeyCode.LeftControl))
            {
                newFOV /= 2f;
                newVignette = 0.3f;
                newBlur = 0.8f;
            }
            float lerpSpeed = 10f * Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newFOV, lerpSpeed);
            vignette.intensity = Mathf.Lerp(vignette.intensity, newVignette, lerpSpeed);
            vignette.blur = Mathf.Lerp(vignette.blur, newBlur, lerpSpeed);
        }
    }

    void OnDestroy()
    {
        if(!isLocalPlayer)
        {
            GameManager.instance.RemovePlayer(this);
        }
    }

    private void UpdateScore(int newScore)
    {
        score = newScore;
        GameManager.instance.UpdateScoreboard();
    }

    private void UpdateUsername(string newUsername)
    {
        username = newUsername;
        name = "Player " + username;
        GameManager.instance.UpdateScoreboard();
    }

    [Command]
    private void CmdSetUsername(string username)
    {
        this.username = username;
    }
}
