using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button BtnHome;
    public FixedButton BtnZoomIn;
    public Button BtnReset;
    public FixedButton BtnZoomOut;
    public FixedButton BtnHint;
    public Button BtnCameraPan;
    
    public GameObject _scrollView;
    private Image btnCameraPanImage;

    public Button BtnPlay;
    public Button BtnNext;

    public Text TextTotalTiles;
    public Text TextTilesInPlace;
    public Text TextTime;

    public Text TextWin;

    public GameObject NonActiveGameObject;
    
    public delegate void DelegateOnClick();
    public DelegateOnClick OnClickHome;
    public DelegateOnClick OnClickZoomIn;
    public DelegateOnClick OnClickReset;
    public DelegateOnClick OnClickZoomOut;
    public DelegateOnClick OnClickPlay;
    public DelegateOnClick OnClickCameraPan;
    
    public DelegateOnClick OnClickNext;
    private CompositeDisposable disposable = new CompositeDisposable();


    // Our game controls when the menu is enabled of disabled.
    // Enabled = false means that the UI won't handle
    // inputs.
    static public bool Enabled { get; set; } = true;

    // Start is called before the first frame update
    private void OnValidate()
    {
        if (BtnCameraPan) btnCameraPanImage = BtnCameraPan.GetComponent<Image>();
    }

    private void Start()
    {
        BtnCameraPan.OnClickAsObservable()
            .Subscribe(_ => OnClickCameraPan?.Invoke())
            .AddTo(disposable);
    }

    private void OnEnable()
    {
        OnClickCameraPan += SetCameraBtnImageColor;
    }

    private void OnDisable()
    {
        OnClickCameraPan -= SetCameraBtnImageColor;
    }

    // Update is called once per frame

    void Update()
    {
        if (!Enabled) return;

        if (BtnZoomIn.Pressed)
        {
            OnClickZoomIn?.Invoke();
        }

        if (BtnZoomOut.Pressed)
        {
            OnClickZoomOut?.Invoke();
        }

        
    }

    public void SetTotalTiles(int count)
    {
        TextTotalTiles.text = count.ToString();
    }

    public void SetTilesInPlace(int count)
    {
        TextTilesInPlace.text = count.ToString();
    }

    public void SetTimeInSeconds(double tt)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(tt);

        string time = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);

        TextTime.text = time;
    }

    public void OnClickBtnHome()
    {
        
        GameManager.Instance.LoadLevelSelectScreen();
    }

    public void OnClickBtnReset()
    {
        if (!Enabled) return;
        OnClickReset?.Invoke();
    }

    public void OnClickBtnPlay()
    {
        OnClickPlay?.Invoke();
    }


    /*public void OnClickBtnNext()
    {
        OnClickNext?.Invoke();
    }
    */

    public void SetActivePlayBtn(bool flag)
    {
        BtnPlay.gameObject.SetActive(flag);
        BtnNext.gameObject.SetActive(flag);
        NonActiveGameObject.SetActive(flag);

        BtnZoomIn.gameObject.SetActive(!flag);
        BtnReset.gameObject.SetActive(!flag);
        BtnZoomOut.gameObject.SetActive(!flag);
        BtnHint.gameObject.SetActive(!flag);
        BtnCameraPan.gameObject.SetActive(!flag);
        
        _scrollView.SetActive(!flag);
    }

    private void SetCameraBtnImageColor()
    {
        btnCameraPanImage.color = CameraMovement.CameraPanning ? Color.white: Color.green;
    }
}
