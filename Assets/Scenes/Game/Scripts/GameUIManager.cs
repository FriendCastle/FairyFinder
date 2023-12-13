using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public interface ITransitioner
{
	public void TransitionFromTo(int argFromIndex, int argToIndex);
}

public class GameUIManager : MonoBehaviour, ITransitioner
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

	public void TransitionFromTo(int argFromIndex, int argToIndex)
	{
		Debug.LogFormat("[UIManager] Transitioning from {0} screen to {1} screen", (ScreenType)argFromIndex, (ScreenType)argToIndex);

		screens[argFromIndex].Hide();
		screens[argToIndex].Show();
	}

#region UI Hookup Classes

	[Serializable]
	public abstract class Panel
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
	public abstract class Screen : ITransitioner
	{
		[SerializeField]
		protected CanvasGroup _screen = null;

		[SerializeField]
		private RectTransform safeAreaRect = null;

		public abstract void Setup();

		public abstract void TransitionFromTo(int argFromIndex, int argToIndex);

		public virtual void Show()
		{
			_screen.gameObject.SetActive(true);
		}

		public virtual void Hide()
		{
			_screen.gameObject.SetActive(false);
		}
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
					GameManager.instance.CreateRoom();
					GameUIManager.instance.TransitionFromTo(ScreenType.Main, ScreenType.Game);
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
					GameManager.instance.JoinRoom(_roomNumberInputField.text);
					GameUIManager.instance.TransitionFromTo(ScreenType.Main, ScreenType.Game);
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