using DG.Tweening;
using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    [Header("Mechanics")]
    private Dictionary<MechanicType, bool> mechanicStatuses;
    private Weapon currWeapon;

    [Header("Hotbar")]
    private ItemSelector itemSelector;

    [Header("Weapons/Primary/Secondary Actions")]
    [SerializeField] private WeaponActionPair[] weaponActionPairs;

    [Header("Barrier")]
    [SerializeField] private SpriteRenderer barrier;
    private MagicMissileSecondaryAction barrierAction;
    private float barrierAlpha;
    private bool isBarrierRetractedPreMax; // for barrier max duration
    private Tweener barrierTweener;
    private Coroutine barrierCoroutine;
    private Coroutine barrierDurationCoroutine;

    [Header("Flamethrower")]
    [SerializeField] private Transform flamethrower;
    private FireSecondaryAction flamethrowerAction;
    private Quaternion initialRot;
    private Coroutine flamethrowerDurationCoroutine;
    private bool isFlamethrowerFlipped;
    private bool isFlamethrowerRetractedPreMax; // for flamethrower max duration

    [Header("Rock")]
    private Rock currRock;
    private bool isRockSummoning;
    private bool isRockThrowReady;
    private bool isRockThrownPreMax; // for rock throw max duration
    private Coroutine rockCoroutine;
    private Coroutine rockThrowDurationCoroutine;

    [Header("Death")]
    private bool isDead; // to deal with death delay

    [Header("Health")]
    private List<WeaponActionPair> deathSubscriptions; // for unsubscribing later

    private new void Awake() {

        base.Awake();

        // set up mechanic statuses early so scripts can change them earlier too
        mechanicStatuses = new Dictionary<MechanicType, bool>();
        Array mechanics = Enum.GetValues(typeof(MechanicType)); // get all mechanic type values

        // add all mechanic types to dictionary
        foreach (MechanicType mechanicType in mechanics)
            mechanicStatuses.Add(mechanicType, true); // set all mechanics to true by default

        itemSelector = FindObjectOfType<ItemSelector>();
        deathSubscriptions = new List<WeaponActionPair>();

        // pick the first secondary action as the default, subscribe to death event, initialize hotbar
        for (int i = 0; i < weaponActionPairs.Length; i++) {

            WeaponActionPair action = weaponActionPairs[i];
            PrimaryAction primaryAction = action.GetPrimaryAction();
            SecondaryAction secondaryAction = action.GetSecondaryAction();

            // enable current primary & secondary action (if they exist), disable the rest
            if (action == weaponActionPairs[0]) {

                if (primaryAction)
                    primaryAction.enabled = true;

                if (secondaryAction)
                    secondaryAction.enabled = true;

            } else {

                if (primaryAction)
                    primaryAction.enabled = false;

                if (secondaryAction)
                    secondaryAction.enabled = false;

            }

            itemSelector.SetSpellData(action.GetSpellData(), i); // add weapon item to hotbar

            if (primaryAction)
                health.OnDeath += primaryAction.OnDeath; // subscribe to death event

            if (secondaryAction)
                health.OnDeath += secondaryAction.OnDeath; // subscribe to death event

            deathSubscriptions.Add(action); // add to list for unsubscribing later

        }
    }

    private void Start() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        slowEffect = GetComponent<SlowEffect>();
        anim = GetComponent<Animator>();

        barrierAction = GetComponent<MagicMissileSecondaryAction>();
        flamethrowerAction = GetComponent<FireSecondaryAction>();

        currWeapon = weaponActionPairs[0].GetWeapon(); // get first weapon
        charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon to first weapon by default

        /* BARRIER */
        barrierAlpha = barrier.color.a;
        barrier.gameObject.SetActive(false); // barrier is not deployed by default

        /* FLAMETHROWER */
        flamethrower.gameObject.SetActive(false); // hide flamethrower particles
        initialRot = flamethrower.transform.rotation;

    }

    private void Update() {

        /* ACTIONS */
        // IMPORTANT: do this before isDead check to prevent toggle issues on death
        currWeapon = null;
        PrimaryAction currPrimaryAction = null;
        SecondaryAction currSecondaryAction = null;

        if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

            currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
            currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
            currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

        }

        /* PRIMARY ACTIONS */
        if (currPrimaryAction) { // make sure slot has a primary action in it

            if (currPrimaryAction.IsRegularAction()) { // primary action is regular action

                if ((currPrimaryAction.IsAutoAction() && Input.GetMouseButton(0)) || // primary action is auto
                    (!currPrimaryAction.IsAutoAction() && Input.GetMouseButtonDown(0))) { // primary action is not auto

                    currPrimaryAction.OnTriggerRegular(); // trigger regular primary action

                }
            } else { // primary action is hold action

                if (Input.GetMouseButtonDown(0)) // start hold
                    currPrimaryAction.OnTriggerHold(true);
                else if (Input.GetMouseButtonUp(0)) // stop hold
                    currPrimaryAction.OnTriggerHold(false);

            }
        }

        /* SECONDARY ACTIONS */
        if (currWeapon && currSecondaryAction) { // make sure slot has a weapon/secondary action in it

            if (currSecondaryAction.IsRegularAction()) { // secondary action is regular action

                if ((currSecondaryAction.IsAutoAction() && Input.GetMouseButton(1)) || // secondary action is auto
                    (!currSecondaryAction.IsAutoAction() && Input.GetMouseButtonDown(1))) { // secondary action is not auto

                    currSecondaryAction.OnTriggerRegular(); // trigger regular secondary action

                }
            } else { // secondary action is hold action

                if (Input.GetMouseButtonDown(1)) // start hold
                    currSecondaryAction.OnTriggerHold(true);
                else if (Input.GetMouseButtonUp(1)) // stop hold
                    currSecondaryAction.OnTriggerHold(false);

            }
        }

        // handle flipping with flamethrower (gets flipped on sprite renderer)
        if (flamethrowerAction.IsFlamethrowerEquipped()) {

            if (spriteRenderer.flipX && !isFlamethrowerFlipped) { // then flip flamethrower

                flamethrower.transform.localPosition = new Vector3(-flamethrower.transform.localPosition.x, flamethrower.transform.localPosition.y, flamethrower.transform.localPosition.z); // flip x axis local position
                flamethrower.transform.rotation *= Quaternion.Euler(0f, 180f, 0f); // flip overlay by adding 180f on the Y axis
                isFlamethrowerFlipped = true;

            } else if (!spriteRenderer.flipX && isFlamethrowerFlipped) { // then unflip flamethrower

                flamethrower.transform.localPosition = new Vector3(-flamethrower.transform.localPosition.x, flamethrower.transform.localPosition.y, flamethrower.transform.localPosition.z); // flip x axis position
                flamethrower.transform.rotation = initialRot; // reset overlay rotation to initial rotation
                isFlamethrowerFlipped = false;

            }
        }

        if (isDead)
            return; // player is dead, no need to update

        /* SCROLL WHEEL WEAPON SWITCHING */
        if (Input.mouseScrollDelta.y > 0f && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) { // make sure barrier is not deployed & flamethrower isn't equipped before switching

            itemSelector.CycleSlot(-1); // cycle hotbar slot backwards

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.mouseScrollDelta.y < 0f && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) { // make sure barrier is not deployed before switching

            itemSelector.CycleSlot(1); // cycle hotbar slot forwards

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        }

        /* KEY WEAPON SWITCHING */
        if (Input.GetKeyDown(KeyCode.Alpha1) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(0);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha2) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(1);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha3) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(2);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha4) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(3);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha5) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(4);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha6) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(5);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha7) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(6);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha8) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(7);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha9) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(8);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha0) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped() && !isRockSummoning && !isRockThrowReady) {

            itemSelector.SelectSlot(9);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currWeapon && currSecondaryAction) // make sure weapon and secondary action exist
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

            }

            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();
                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currWeapon && currSecondaryAction) { // make sure weapon and secondary action exist

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        }
    }

    protected new void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water") && !barrierAction.IsBarrierDeployed())  // barrier can save player from water
            health.Kill();

    }

    protected new void OnDisable() {

        base.OnDisable();

        // unsubscribe from all events
        foreach (WeaponActionPair action in deathSubscriptions) {

            if (action.GetPrimaryAction())
                health.OnDeath -= action.GetPrimaryAction().OnDeath;

            if (action.GetSecondaryAction())
                health.OnDeath -= action.GetSecondaryAction().OnDeath;

        }
    }

    #region BARRIER

    public void DeployBarrier(float maxDuration) {

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        barrierCoroutine = StartCoroutine(HandleDeployBarrier());

        charWeaponHandler.CurrentWeapon.gameObject.SetActive(false); // hide weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)

        isBarrierRetractedPreMax = false; // barrier is not retracted yet (for max duration)
        barrierDurationCoroutine = StartCoroutine(HandleBarrierDuration(maxDuration)); // handle barrier max duration

    }

    private IEnumerator HandleDeployBarrier() {

        DisableAllMechanics(); // disable all mechanics while barrier is being deployed (except secondary action)
        EnableMechanic(MechanicType.SecondaryAction); // enable only secondary action while barrier is deployed
        DisableCoreScripts(); // disable all scripts while barrier is deployed (including weapon handler)

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, 0f); // set barrier alpha to none
        barrier.gameObject.SetActive(true); // show barrier
        anim.SetBool("isBarrierDeployed", true); // play barrier deploy animation
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(barrierAlpha, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.InBounce).OnComplete(() => barrierCoroutine = null); // fade barrier in based on animation length

    }

    public void RetractBarrier() {

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        if (barrierDurationCoroutine != null) StopCoroutine(barrierDurationCoroutine); // stop barrier duration coroutine if it's running

        charWeaponHandler.CurrentWeapon.gameObject.SetActive(true); // show weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)

        isBarrierRetractedPreMax = true; // barrier is retracted (for max duration)
        barrierCoroutine = StartCoroutine(HandleRetractBarrier());

        /* the following is done without a fade animation */
        //barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        //anim.SetBool("isBarrierDeployed", false);
        //barrier.gameObject.SetActive(false); // hide barrier
        //isBarrierDeployed = false;
        //EnableAllMechanics(); // enable all mechanics after barrier is retracted

    }

    private IEnumerator HandleRetractBarrier() {

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        anim.SetBool("isBarrierDeployed", false);
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(0f, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.OutBounce).OnComplete(() => {

            barrier.gameObject.SetActive(false); // hide barrier
            EnableCoreScripts(); // enable all scripts after barrier is retracted (including weapon handler)
            EnableAllMechanics(); // enable all mechanics after barrier is retracted
            barrierCoroutine = null;

        }); // fade barrier in based on animation length
    }

    private IEnumerator HandleBarrierDuration(float maxDuration) {

        float timer = 0f;

        while (timer < maxDuration) {

            if (isBarrierRetractedPreMax) { // barrier is retracted before max duration

                isBarrierRetractedPreMax = false; // reset retracted status
                barrierDurationCoroutine = null;
                yield break;

            }

            timer += Time.deltaTime;
            yield return null;

        }

        RetractBarrier();
        barrierDurationCoroutine = null;

    }

    #endregion

    #region FLAMETHROWER

    public void EquipFlamethrower(float maxDuration) {

        charWeaponHandler.CurrentWeapon.gameObject.SetActive(false); // hide weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)
        isFlamethrowerRetractedPreMax = false; // flamethrower is not unequipped yet (for max duration)

        DisableAllMechanics(); // disable all mechanics while flamethrower is being equipped (except secondary action)
        EnableMechanic(MechanicType.SecondaryAction); // enable only secondary action while flamethrower is equipped

        flamethrower.gameObject.SetActive(true); // show flamethrower particles
        flamethrowerDurationCoroutine = StartCoroutine(HandleFlamethrowerDuration(maxDuration)); // handle flamethrower max duration

    }

    public void UnequipFlamethrower() {

        if (flamethrowerDurationCoroutine != null) StopCoroutine(flamethrowerDurationCoroutine); // stop flamethrower duration coroutine if it's running

        charWeaponHandler.CurrentWeapon.gameObject.SetActive(true); // show weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)
        currWeapon.gameObject.SetActive(true); // show weapon

        isFlamethrowerRetractedPreMax = true; // flamethrower is unequipped (for max duration)

        EnableAllMechanics(); // enable all mechanics after flamethrower is unequipped

        flamethrower.gameObject.SetActive(false); // hide flamethrower particles

    }

    private IEnumerator HandleFlamethrowerDuration(float maxDuration) {

        float timer = 0f;

        while (timer < maxDuration) {

            if (isFlamethrowerRetractedPreMax) { // flamethrower is retracted before max duration

                isFlamethrowerRetractedPreMax = false; // reset retracted status
                yield break;

            }

            timer += Time.deltaTime;
            yield return null;

        }

        UnequipFlamethrower();
        flamethrowerDurationCoroutine = null;

    }

    #endregion

    #region ROCK

    // returns true if rock is successfully summoned, false if rock is already summoned
    public bool SummonRock(EarthPrimaryAction action, Rock rockPrefab, float maxThrowDuration) {

        if (currRock) return false; // rock is already summoned

        if (rockCoroutine != null) StopCoroutine(rockCoroutine); // stop rock coroutine if it's running

        rockCoroutine = StartCoroutine(HandleSummonRock(action, rockPrefab, maxThrowDuration));

        return true;

    }

    private IEnumerator HandleSummonRock(EarthPrimaryAction action, Rock rockPrefab, float maxThrowDuration) {

        isRockSummoning = true;
        DisableAllMechanics(); // disable all mechanics while rock is being summoned (except primary action)
        EnableMechanic(MechanicType.PrimaryAction); // enable only primary action while rock is summoned
        DisableCoreScripts(); // disable all scripts while rock is being summoned (including weapon handler)

        currRock = Instantiate(rockPrefab, transform.position, Quaternion.identity); // instantiate rock (will play summon animation automatically)
        anim.SetBool("isRockSummoned", true); // play rock summon animation

        yield return null; // wait for animation to start
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length); // wait for animation to end (so rock can be dropped during animation because this coroutine won't be null)

        isRockSummoning = false;
        isRockThrowReady = true; // rock is ready to be thrown
        SetWeaponHandlerEnabled(true); // enable weapon handler when rock is fully summoned
        action.ActivateWeapon(); // activate weapon after rock is summoned

        isRockThrownPreMax = false; // rock has not been thrown yet (for max duration)
        rockThrowDurationCoroutine = StartCoroutine(HandleRockThrowDuration(maxThrowDuration)); // handle rock throw max duration

        rockCoroutine = null;

    }

    public void DropRock() {

        if (!currRock || !isRockSummoning) return; // no rock to drop or rock has been fully summoned already -> can't drop it

        if (rockCoroutine != null) StopCoroutine(rockCoroutine); // stop rock coroutine if it's running

        DestroyRock(true);

    }

    public void OnRockThrow() {

        if (!currRock || isRockSummoning) return; // no rock to destroy or rock is still being summoned -> can't throw it

        if (rockCoroutine != null) StopCoroutine(rockCoroutine); // stop rock coroutine if it's running

        if (rockThrowDurationCoroutine != null) StopCoroutine(rockThrowDurationCoroutine); // stop rock throw duration coroutine if it's running

        isRockThrownPreMax = true; // rock has been thrown (for max duration)
        rockCoroutine = StartCoroutine(HandleRockThrow()); // handle rock throw

    }

    private IEnumerator HandleRockThrow() {

        DestroyRock(false); // don't activate mechanics because it is dealt with differently in this case

        // must wait for two frames to allow shot to be fired
        yield return null;
        yield return null;

        charWeaponHandler.ShootStop(); // stop shooting weapon (to deal with infinite shooting bug | do this before disabling the core scripts)
        EnableCoreScripts(); // enable all scripts after rock is dropped/thrown (except weapon handler | don't want player to use weapon unless rock is fully summoned)
        SetWeaponHandlerEnabled(false); // disable weapon handler when rock is thrown
        EnableAllMechanics(); // enable all mechanics after rock is dropped/thrown

        rockCoroutine = null;

    }

    private void DestroyRock(bool activateMechanics) {

        // destroy rock
        Destroy(currRock.gameObject);
        currRock = null;

        corgiController.SetForce(Vector2.zero); // stop player movement
        anim.SetBool("isRockSummoned", false);

        // reset bools
        isRockSummoning = false;
        isRockThrowReady = false;

        if (activateMechanics) {

            EnableCoreScripts(); // enable all scripts after rock is dropped/thrown (except weapon handler | don't want player to use weapon unless rock is fully summoned)
            SetWeaponHandlerEnabled(false); // disable weapon handler when rock is dropped
            EnableAllMechanics(); // enable all mechanics after rock is dropped/thrown

        }
    }

    private IEnumerator HandleRockThrowDuration(float maxThrowDuration) {

        float timer = 0f;

        while (timer < maxThrowDuration) {

            if (isRockThrownPreMax) { // barrier is retracted before max duration

                isRockThrownPreMax = false; // reset retracted status
                rockThrowDurationCoroutine = null;
                yield break;

            }

            timer += Time.deltaTime;
            yield return null;

        }

        DestroyRock(true);
        rockThrowDurationCoroutine = null;

    }

    #endregion

    #region MECHANICS

    public void EnableAllMechanics() {

        // enable all mechanics
        foreach (MechanicType mechanicType in mechanicStatuses.Keys.ToList())
            mechanicStatuses[mechanicType] = true;

    }

    public void EnableMechanic(MechanicType mechanicType) => mechanicStatuses[mechanicType] = true;

    public void DisableAllMechanics() {

        // disable all mechanics
        foreach (MechanicType mechanicType in mechanicStatuses.Keys.ToList())
            mechanicStatuses[mechanicType] = false;

        // send to idle animation
        anim.SetBool("isMoving", false); // stop moving animation

    }

    public void DisableMechanic(MechanicType mechanicType) => mechanicStatuses[mechanicType] = false;

    public bool IsMechanicEnabled(MechanicType mechanicType) => mechanicStatuses[mechanicType];

    #endregion

    #region UTILITIES

    public bool IsGrounded() => corgiController.State.IsGrounded;

    public Animator GetAnimator() => anim;

    protected override void OnRespawn() {

        base.OnRespawn();
        isDead = false;

    }

    protected override void OnDeath() {

        base.OnDeath();

        isDead = true;

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running
        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        flamethrower.gameObject.SetActive(false); // hide flamethrower particles

    }

    #endregion
}
