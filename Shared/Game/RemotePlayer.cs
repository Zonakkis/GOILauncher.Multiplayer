using GOILauncher.Multiplayer.Shared.Constants;
using GOILauncher.Multiplayer.Shared.Extensions;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Game
{
    public class RemotePlayer : PlayerBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RuntimePlatform Platform { get; set; }

        private float _timer;

        public void LateUpdate()
        {
            if (NextMove == null)
                return;
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
        public void OnGUI()
        {
            Vector2 screenPosition = Camera.current.WorldToScreenPoint(transform.position + transform.up * 1.5f);
            screenPosition.y = Screen.height - screenPosition.y;
            var nameContent = new GUIContent($"[{Id}][{Platform.ToReadableString()}]{Name}");
            var textSize = GUI.skin.label.CalcSize(nameContent);
            if (screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x >= Screen.width || screenPosition.y >= Screen.height)
            {
                var cameraPosition = Camera.main.transform.position;
                cameraPosition.z = 0;
                nameContent.text += $" ({Vector3.Distance(cameraPosition, transform.position):0.0}m)";
                textSize = GUI.skin.label.CalcSize(nameContent);
                screenPosition.x = Mathf.Clamp(screenPosition.x, textSize.x / 2, Screen.width - textSize.x / 2);
                screenPosition.y = Mathf.Clamp(screenPosition.y, textSize.y / 2, Screen.height - textSize.y / 2);
            }
            var textRect = new Rect(screenPosition.x - (textSize.x / 2f), screenPosition.y - (textSize.y / 2f), textSize.x, textSize.y);
            var shadowRect = new Rect(textRect);
            shadowRect.position += Vector2.one;
            var oldColor = GUI.color;
            GUI.color = Color.black;
            GUI.Label(shadowRect, nameContent);
            GUI.color = Color.white;
            GUI.Label(textRect, nameContent);
            GUI.color = oldColor;
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
