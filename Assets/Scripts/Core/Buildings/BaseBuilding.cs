﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Helpers;

public abstract class BaseBuilding : PurchasableObject {
    #region Class Data Structures
    public struct BuildingLevel
    {
        public int MaxHitPoints;      // maximum number of HP for this structure at this level
        public BuildRequirement BuildCost;    // resource and other structure requirements
    }
    #endregion

    protected BuildingLevel _buildingLevel;

    [Header("Building Setup")]
    [SerializeField] private GameObject _constructionGraphic;
    [SerializeField]
    private GameObject _upgradeGraphic;

    [SerializeField]
    private bool _requiresBuilder = true;

	[SerializeField]
    protected GameObject _model;

    protected float _collisionRadiusSquared;

    protected int _currentLevel;

    protected bool _isAlive = true;


    #region MonoBehaviour Methods

    protected override void OnEnable()
    {
        base.OnEnable();
		TimerManager.onTimerExpired += new TimerManager.TimerExpiredDelegate(onTimerExpired);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
		TimerManager.onTimerExpired -= new TimerManager.TimerExpiredDelegate(onTimerExpired);
    }

    protected virtual void onTimerExpired(TimerManager.TimerEventData eventData)
    {
        if (eventData.id == UID + "_build")
        {
            BuildComplete(eventData.EndTime);
        }
    }

    #endregion

    #region Core Methods

	protected override void StartBuild()
    {
		if (BuildingCost.BuildTime > 0)
        {
            if (_requiresBuilder) AssignBuilder();

            TimerManager.Instance.StartTimerNow(UID + "_build", BuildingCost.BuildTime);
			UIManager.Instance.Get<UIBuildTimersPanel>(UIManager.PanelID.BuildTimersPanel).AddTimer(UID + "_build", UIAnchor, Vector3.up * 2);
            if (_constructionGraphic != null)
            {
                _constructionGraphic.SetActive(true);
                Model.gameObject.SetActive(false);
            }
            if (_upgradeGraphic != null)
            {
                _upgradeGraphic.SetActive(true);
            }

            BuildStarted();
        }
        else
        {
			BuildComplete(DateTime.Now);
        }
    }

	public override void OnSpawned ()
	{
		base.OnSpawned ();

		Vector3 modelLocalPosition = _collider.transform.localPosition;
		modelLocalPosition.y = 0;
		_model.transform.localPosition = modelLocalPosition;
	}

	protected virtual void BuildStarted() {}

    public virtual void BuildComplete(System.DateTime timeCompleted)
    {
        if (_constructionGraphic != null)
        {
            _constructionGraphic.SetActive(false);
        }

        Model.gameObject.SetActive(true);

        //if this was called because of skipping rather than waiting, cancel the exisiting timer.
        if (TimerManager.Instance.IsTimerRunning(UID + "_build"))
        {
            TimerManager.Instance.CancelTimer(UID + "_build");
        }

        ReturnBuilder();

        UIManager.Instance.Get<UIBuildTimersPanel>(UIManager.PanelID.BuildTimersPanel).RemoveTimer(UID + "_build");

		AudioManager.Instance.Play(AudioSFXDatabase.Instance.BuildSFX, AudioManager.AudioGroup.Building_Created);
    }

    #endregion

    #region Public Accessors

    public bool IsBuilding
    {
        get { return TimerManager.Instance.IsTimerRunning(UID + "_build"); }
    }

    public override float ConstructionTimeRemaining
    {
        get
        {
            float timeRemaining = 0;
            if (IsBuilding)
            {
                timeRemaining = (float)TimerManager.Instance.GetTimerLeftDateTime(UID + "_build").TotalSeconds;
            }

            return timeRemaining;
        }
    }

	public override BuildRequirement BuildingCost
    {
        get { return _buildingLevel.BuildCost; }
    }

    public bool RequiresBuilder
    {
        get { return _requiresBuilder; }
    }

    public GameObject Model
    {
        get
        {
            return _model;
        }
    }
    #endregion
}
