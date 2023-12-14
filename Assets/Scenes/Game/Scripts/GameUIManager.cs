using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public interface ITransitioner
{
	public void TransitionTo(int argToIndex);
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

	private int currentScreen;

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

	public void TransitionTo(int argToIndex)
	{
		Debug.LogFormat("[UIManager] Transitioning from {0} screen to {1} screen", (ScreenType)currentScreen, (ScreenType)argToIndex);

		screens[currentScreen].Hide();
		screens[argToIndex].Show();

		currentScreen = argToIndex;
	}

	public void UpdateRoomCode()
	{
		_gameScreen.UpdateRoomCode();
	}

	public void TransitionToGamePanel()
	{
		_gameScreen.TransitionTo(GameScreen.PanelType.Game);
	}

	public void UpdatePlayerNumberText(string argPlayerNumber)
	{
		_gameScreen.UpdatePlayerNumberText(argPlayerNumber);
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

		protected Panel[] panels;

		protected int currentPanel;

		protected abstract void InitializePanels();

		public virtual void Setup()
		{
			InitializePanels();

			foreach (var panel in panels)
			{
				panel.Setup(this);
			}
		}

		public virtual void TransitionTo(int argToIndex)
		{
			panels[currentPanel].Hide();
			panels[argToIndex].Show();

			currentPanel = argToIndex;
		}

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

		protected override void InitializePanels()
		{
			panels = new Panel[]{ _mainPanel, _createRoomPanel, _joinRoomPanel };
		}

		public override void TransitionTo(int argToIndex)
		{
			Debug.LogFormat("[UIManager] Transitioning Main Menu Screen from {0} to {1}", (PanelType)currentPanel, (PanelType)argToIndex);

			base.TransitionTo(argToIndex);
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
					panelOwner.TransitionTo(PanelType.CreateRoom);
				});

				_joinRoomButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionTo(PanelType.JoinRoom);
				});
			}
		}

		[Serializable]
		private class CreateRoomPanel : Panel
		{
			[SerializeField]
			private TMP_Text _playerCountText = null;

			[SerializeField]
			private Button _playerCountButtonLeft = null;

			[SerializeField]
			private Button _playerCountButtonRight = null;

			[SerializeField]
			private Button _startRoomButton = null;

			[SerializeField]
			private Button _backButton = null;

			private int visualPlayerCount = GameManager.MIN_PLAYER_COUNT;

			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);

				_startRoomButton.onClick.AddListener(delegate
				{
					GameManager.instance.CreateRoom(visualPlayerCount);
					GameUIManager.instance.TransitionTo(ScreenType.Game);
				});

				_backButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionTo(PanelType.Main);
				});

				_playerCountButtonLeft.onClick.AddListener(delegate
				{
					visualPlayerCount = Math.Clamp(visualPlayerCount - 1, GameManager.MIN_PLAYER_COUNT, GameManager.MAX_PLAYER_COUNT);
					_playerCountText.text = visualPlayerCount.ToString();
				});

				_playerCountButtonRight.onClick.AddListener(delegate
				{
					visualPlayerCount = Math.Clamp(visualPlayerCount + 1, GameManager.MIN_PLAYER_COUNT, GameManager.MAX_PLAYER_COUNT);
					_playerCountText.text = visualPlayerCount.ToString();
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
					GameUIManager.instance.TransitionTo(ScreenType.Game);
				});

				_backButton.onClick.AddListener(delegate
				{
					panelOwner.TransitionTo(PanelType.Main);
				});
			}
		}
	}

	[Serializable]
	private class GameScreen : Screen, ITransitioner
	{
		public enum PanelType
		{
			Scan,
			Game
		}

		[SerializeField]
		private TMP_Text _roomCodeText;

		[SerializeField]
		private ScanImagePanel _scanImagePanel;

		[SerializeField]
		private LobbyPanel _lobbyPanel;

		[SerializeField]
		private GamePanel _gamePanel;

		[SerializeField]
		private GameEndPanel _gameEndPanel;

		protected override void InitializePanels()
		{
			panels = new Panel[]{ _scanImagePanel, _gamePanel };
		}

		public override void TransitionTo(int argToIndex)
		{
			Debug.LogFormat("[UIManager] Transitioning Game Screen from {0} to {1}", (PanelType)currentPanel, (PanelType)argToIndex);

			base.TransitionTo(argToIndex);
		}

		public void UpdateRoomCode()
		{
			_roomCodeText.text = string.Format("Room Code:\n{0}", GameManager.instance.roomName);
		}

		public void UpdatePlayerNumberText(string argPlayerNumber)
		{
			_gamePanel.UpdatePlayerNumberText(argPlayerNumber);
		}

		[Serializable]
		private class ScanImagePanel : Panel
		{
			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);
			}
		}


		[Serializable]
		private class LobbyPanel : Panel
		{
			[SerializeField]
			private TMP_Text _instructionText;

			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);
			}
		}

		[Serializable]
		private class GamePanel : Panel
		{
			[SerializeField]
			private TMP_Text _playerNumberText;

			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);
			}

			public void UpdatePlayerNumberText(string argText)
			{
				_playerNumberText.text = argText;
			}
		}

		[Serializable]
		private class GameEndPanel : Panel
		{
			public override void Setup(Screen argPanelOwner)
			{
				base.Setup(argPanelOwner);
			}
		}
	}
#endregion // UI Hookup Classes
}