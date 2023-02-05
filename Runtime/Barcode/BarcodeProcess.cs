using System;
using UnityEngine;
using UnityEngine.Android;
using ZXing;

namespace MokomoGamesLib.Runtime.Barcode
{
    public class BarcodeProcess
    {
        private readonly BarcodeReader _barcodeReader;
        private bool isPlayingRecordCam;

        public BarcodeProcess()
        {
            Permission.RequestUserPermission(Permission.Camera);

            _barcodeReader = new BarcodeReader
            {
                Options =
                {
                    TryHarder = false
                }
            };
        }

        public WebCamTexture WebCamTexture { get; private set; }

        public event Action OnStartRecording;

        public void Tick(float deltaTime)
        {
            if (WebCamTexture == null)
                if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    WebCamTexture = new WebCamTexture(Screen.width, Screen.height);
                    WebCamTexture.Play();
                }

            if (!isPlayingRecordCam)
            {
                OnStartRecording?.Invoke();
                isPlayingRecordCam = true;
            }
        }

        public string Decode(WebCamTexture texture)
        {
            var result = _barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);
            return result != null ? result.Text : string.Empty;
        }
    }
}