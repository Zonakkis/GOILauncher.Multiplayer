using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Extensions;
using GOILauncher.Multiplayer.Shared.Game;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client.Game
{
    public class RemotePlayer : PlayerBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RuntimePlatform Platform { get; set; }
        public Transform LocalPlayer { get; set; }

        private float _timer;
        private readonly Camera _camera = Camera.main;
        private Vector2 _topLeft;
        private Vector2 _bottomRight;
        private bool _isInCamera;
        private GUIStyle _labelStyle;

        public void LateUpdate()
        {
            if (NextMove != null)
            {
                _timer += Time.deltaTime;
                if (_timer <= GameConstants.FrameTime)
                    ApplyMove(_timer / GameConstants.FrameTime);
                else
                {
                    _timer -= GameConstants.FrameTime;
                    ApplyMove(NextMove);
                    Move = NextMove;
                    NextMove = null;
                }
            }
            var cameraPosition = _camera.transform.position;
            var halfHeight = _camera.orthographicSize;
            var halfWidth = _camera.aspect * halfHeight;
            _topLeft = new Vector2(cameraPosition.x - halfWidth, cameraPosition.y + halfHeight);
            _bottomRight = new Vector2(cameraPosition.x + halfWidth, cameraPosition.y - halfHeight);
            _isInCamera = Player.position.IsInRectangle(
                _topLeft.x - GameConstants.PlayerWidth, _topLeft.y + GameConstants.PlayerHeight,
                _bottomRight.x + GameConstants.PlayerWidth, _bottomRight.y - GameConstants.PlayerWidth);
            SetRenderersEnabled(_isInCamera);
        }
        public void OnGUI()
        {
            if (_labelStyle == null)
                _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = (int)Mathf.Clamp(GameConstants.DefaultFontSize
                * (Screen.width / GameConstants.DefaultScreenWidth)
                * (Screen.dpi / GameConstants.DefaultScreenDpi )
                * (GameConstants.DefaultCameraOrthographicSize / _camera.orthographicSize), 10f, 25f);
            var label = new GUIContent(
                _isInCamera ? $"[{Id}][{Platform.ToReadableString()}]{Name}" :
                $"[{Id}][{Platform.ToReadableString()}]{Name}" +
                $"（{Vector3.Distance(Player.position, LocalPlayer.position):0.0}m）");
            var labelSize = _labelStyle.CalcSize(label);
            Vector2 labelScreenPosition = _camera.WorldToScreenPoint(Player.position + Player.up * 1.5f);
            labelScreenPosition.y = Screen.height - labelScreenPosition.y;
            var halfWidth = labelSize.x / 2f;
            var halfHeight = labelSize.y / 2f;
            labelScreenPosition.x = Mathf.Clamp(
                labelScreenPosition.x, halfWidth, Screen.width - halfWidth);
            labelScreenPosition.y = Mathf.Clamp(
                labelScreenPosition.y, halfHeight, Screen.height - halfHeight);
            var labelRect = new Rect(
                labelScreenPosition.x - halfWidth, labelScreenPosition.y - halfHeight,
                labelSize.x, labelSize.y);
            var shadowRect = labelRect;
            shadowRect.position += Vector2.one;
            _labelStyle.normal.textColor = Color.black;
            GUI.Label(shadowRect, label, _labelStyle);
            _labelStyle.normal.textColor = Color.white;
            GUI.Label(labelRect, label, _labelStyle);
        }

        private void ApplyMove(float t)
        {
            Player.position = Vector3.Lerp(Move.PlayerPosition, NextMove.PlayerPosition, t);
            transform.rotation = Quaternion.Slerp(Move.PlayerRotation, NextMove.PlayerRotation, t);
            Handle.position = Vector3.Lerp(Move.HandlePosition, NextMove.HandlePosition, t);
            Handle.rotation = Quaternion.Slerp(Move.HandleRotation, NextMove.HandleRotation, t);
            Slider.position = Vector3.Lerp(Move.SliderPosition, NextMove.SliderPosition, t);
            Slider.rotation = Quaternion.Slerp(Move.SliderRotation, NextMove.SliderRotation, t);
        }

        public override void SetNextMove(Move move)
        {
            NextMove = move;
            ApplyMove(move);
            _timer = 0;
        }
    }
}
