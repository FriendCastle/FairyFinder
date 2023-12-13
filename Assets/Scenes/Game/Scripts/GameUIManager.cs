using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
	public static GameUIManager gameUIManager { get; private set; }

	[Header("UI Hookups")]

	[SerializeField]
	private MainMenuScreen _mainMenuScreen = null;

	[SerializeField]
	private GameScreen _gameScreen = null;

	private Screen[] screens;

	private void Awake()
	{
		gameUIManager = this;

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
	private class Panel
	{
		[SerializeField]
		protected CanvasGroup _panel = null;

		public virtual void Setup()
		{
		}
	}

	[Serializable]
	private class Screen
	{
		[SerializeField]
		protected CanvasGroup _screen = null;

		[SerializeField]
		private RectTransform safeAreaRect = null;

		public virtual void Setup()
		{
		}
	}

	[Serializable]
	private class MainMenuScreen : Screen
	{
		[SerializeField]
		private MainPanel _mainPanel = null;

		[SerializeField]
		private CreateRoomPanel _createRoomPanel = null;


		[SerializeField]
		private JoinRoomPanel _joinRoomPanel = null;

		private Panel[] panels;

		public override void Setup()
		{
			base.Setup();

			panels = new Panel[]{ _mainPanel, _createRoomPanel, _joinRoomPanel };

			foreach (var panel in panels)
			{
				panel.Setup();
			}
		}

		[Serializable]
		private class MainPanel : Panel
		{
			[SerializeField]
			private Button _createRoomButton = null;

			[SerializeField]
			private Button _joinRoomButton = null;

			public override void Setup()
			{
				base.Setup();

				_createRoomButton.onClick.AddListener(delegate
				{
					
				});

				_joinRoomButton.onClick.AddListener(delegate
				{
					
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
		}
	}

	[Serializable]
	private class GameScreen : Screen
	{
	}

#endregion // UI Hookup Classes
}