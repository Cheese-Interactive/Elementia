using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour {

    [Header("References")]
    protected CorgiController corgiController;
    protected Health health;
    protected CharacterHorizontalMovement charMovement;
    protected AIWalk aiWalk;
    protected CharacterCrouch charCrouch;
    protected CharacterDash charDash;
    protected CharacterDive charDive;
    protected CharacterDangling charDangling;
    protected CharacterJump charJump;
    protected CharacterLookUp charLookUp;
    protected CharacterGrip charGrip;
    protected CharacterWallClinging charWallCling;
    protected CharacterWalljump charWallJump;
    protected CharacterLadder charLadder;
    protected CharacterButtonActivation charButton;
    protected DamageOnTouch damageOnTouch;
    protected SlowEffect slowEffect;
    protected CharacterHandleWeapon charWeapon;

    protected void Start() {

        corgiController = GetComponent<CorgiController>();
        health = GetComponent<Health>();
        charMovement = GetComponent<CharacterHorizontalMovement>();
        aiWalk = GetComponent<AIWalk>();
        charCrouch = GetComponent<CharacterCrouch>();
        charDash = GetComponent<CharacterDash>();
        charDive = GetComponent<CharacterDive>();
        charDangling = GetComponent<CharacterDangling>();
        charJump = GetComponent<CharacterJump>();
        charLookUp = GetComponent<CharacterLookUp>();
        charGrip = GetComponent<CharacterGrip>();
        charWallCling = GetComponent<CharacterWallClinging>();
        charWallJump = GetComponent<CharacterWalljump>();
        charLadder = GetComponent<CharacterLadder>();
        charButton = GetComponent<CharacterButtonActivation>();
        damageOnTouch = GetComponent<DamageOnTouch>();
        slowEffect = GetComponent<SlowEffect>();
        charWeapon = GetComponent<CharacterHandleWeapon>();

        health.OnDeath += slowEffect.RemoveEffect; // remove slow effect on death
        health.OnDeath += OnDeath;
        health.OnRevive += OnRespawn;

    }

    protected void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water"))
            health.Kill();

    }

    protected void OnDisable() => health.OnDeath -= slowEffect.RemoveEffect; // remove subscription to on death event

    protected void OnDestroy() => health.OnDeath -= slowEffect.RemoveEffect; // remove subscription to on death event


    #region UTILITIES

    public void EnableCoreMechanics() {

        if (corgiController)
            corgiController.enabled = true;

        if (charMovement)
            charMovement.AbilityPermitted = true;

        if (aiWalk)
            aiWalk.enabled = true;

        if (charCrouch)
            charCrouch.AbilityPermitted = true;

        if (charDash)
            charDash.AbilityPermitted = true;

        if (charDive)
            charDive.AbilityPermitted = true;

        if (charDangling)
            charDangling.AbilityPermitted = true;

        if (charJump)
            charJump.AbilityPermitted = true;

        if (charLookUp)
            charLookUp.AbilityPermitted = true;

        if (charGrip)
            charGrip.AbilityPermitted = true;

        if (charWallCling)
            charWallCling.AbilityPermitted = true;

        if (charWallJump)
            charWallJump.AbilityPermitted = true;

        if (charLadder)
            charLadder.AbilityPermitted = true;

        if (charButton)
            charButton.AbilityPermitted = true;

        if (charWeapon)
            charWeapon.AbilityPermitted = true;

    }

    public void DisableCoreMechanics() {

        if (corgiController)
            corgiController.enabled = false;

        if (charMovement)
            charMovement.AbilityPermitted = false;

        if (aiWalk)
            aiWalk.enabled = false;

        if (charCrouch)
            charCrouch.AbilityPermitted = false;

        if (charDash)
            charDash.AbilityPermitted = false;

        if (charDive)
            charDive.AbilityPermitted = false;

        if (charDangling)
            charDangling.AbilityPermitted = false;

        if (charJump)
            charJump.AbilityPermitted = false;

        if (charLookUp)
            charLookUp.AbilityPermitted = false;

        if (charGrip)
            charGrip.AbilityPermitted = false;

        if (charWallCling)
            charWallCling.AbilityPermitted = false;

        if (charWallJump)
            charWallJump.AbilityPermitted = false;

        if (charLadder)
            charLadder.AbilityPermitted = false;

        if (charButton)
            charButton.AbilityPermitted = false;

        if (charWeapon)
            charWeapon.AbilityPermitted = false;

    }

    protected virtual void OnRespawn() { }

    protected virtual void OnDeath() { }

    #endregion
}
