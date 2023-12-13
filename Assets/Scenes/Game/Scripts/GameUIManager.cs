using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
	public static GameUIManager instance { get; private set; }

	[Header("UI Hookups")]

	[SerializeField]
	private MainMenuScreen _mainMenuScreen = null;

	[SerializeField]
	private GameScreen _gameScreen = null;

	private Screen[] screens;

	public enum ScreenType
	{
		Main,
		Game
	}

	private void Awake()
	{
		instance = this;

		screens = new Screen[]{ _mainMenuScreen, _gameScreen};
	}

	private void Start()
	{
		foreach (var screen in screens)
		{
			screen.Setup();
		}
	}

#region UI Hookup Classes

	[Serializable]
	private abstract class Panel
	{
		[SerializeField]
		protected CanvasGroup _panel = null;

		protected Screen panelOwner;

		public virtual void Setup(Screen argPanelOwner)
		{
			panelOwner = argPanelOwner;
		}

		public virtual void Show()
		{
			_panel.gameObject.SetActive(true);
		}
		public virtual void Hide()
		{
			_panel.gameObject.SetActive(false);
		}
	}

	[Serializable]
	private abstract class Screen
	{
		[SerializeField]
		protected CanvasGroup _screen = null;

		[SerializeField]
		private RectTransform safeAreaRect = null;

		public abstract void Setup();

		public virtual void TransitionFromTo<T>(T argFromIndex, T argToIndex) where T : Enum
		{
			TransitionFromTo(Convert.ToInt32(argFromIndex), Convert.ToInt32(argToIndex));
		}

		public abstract void TransitionFromTo(int argFromIndex, int argToIndex);
	}

	[Serializable]
	private class MainMenuScreen : Screen
	{
		public enum PanelType
		{
			Main,
			CreateRoom,
			JoinRoom
		}

		[SerializeField]
		private MainPanel _mainPanel = null;

		[SerializeField]
		private CreateRoomPanel _createRoomPanel = null;


		[SerializeField]
		private JoinRoomPanel _joinRoomPanel = null;

		private Panel[] panels;

		public override void Setup()
		{
			panels = new Panel[]{ _mainPanel, _createRoomPanel, _joinRoomPanel };

			foreach (var panel in panels)
			{
				panel.Setup(this);
			}
		}

		public override void TransitionFromTo(int argFromIndex, int argToIndex)
		{
			Debug.LogFormat("[UIManager] Transitioning Main Menu Screen from {0} to {1}", (PanelType)argFromIndex, (PanelType)argToIndex);

			panels[argFromIndex].Hide();
			panels[argToIndex].Show();
		}

		[Serializable]
		private class MainPanel : Panel
		{
			[SerializeField]
			private Button _createRoomButton = null;

			[SerializeField]
			private Button _joinRoomButton = null;



			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);

				_createRoomButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionFromTo(PanelType.Main, PanelType.CreateRoom);
				});

				_joinRoomButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionFromTo(PanelType.Main, PanelType.JoinRoom);
				});
			}
		}

		[Serializable]
		private class CreateRoomPanel : Panel
		{
			[SerializeField]
			private RectTransform _playerCountText = null;

			[SerializeField]
			private Button _playerCountButtonLeft = null;

			[SerializeField]
			private Button _playerCountButtonRight = null;


			[SerializeField]
			private Button _startRoomButton = null;

			[SerializeField]
			private Button _backButton = null;

			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);

				_startRoomButton.onClick.AddListener(delegate
				{
					GameNetcodeManager.instance.StartNewRoom();
				});

				_backButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionFromTo(PanelType.CreateRoom, PanelType.Main);
				});
			}
		}

		[Serializable]
		private class JoinRoomPanel : Panel
		{
			[SerializeField]
			private TMP_InputField _roomNumberInputField = null;


			[SerializeField]
			private Button _joinRoomButton = null;

			[SerializeField]
			private Button _backButton = null;

			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);

				_joinRoomButton.onClick.AddListener(delegate
				{
					GameNetcodeManager.instance.Join(_roomNumberInputField.text);
				});

				_backButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionFromTo(PanelType.JoinRoom, PanelType.Main);
				});
			}
		}
	}

	[Serializable]
	private class GameScreen : Screen
	{
		public override void Setup()
		{
		}

		public override void TransitionFromTo(int argFromIndex, int argToIndex)
		{
		}
	}
#endregion // UI Hookup Classes
}