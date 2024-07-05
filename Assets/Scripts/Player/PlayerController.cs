using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("Mechanics")]
    private Weapon currWeapon;

    [Header("Hotbar")]
    private ItemSelector itemSelector;

    [Header("Weapons/Primary/Secondary Actions")]
    [SerializeField] private WeaponActionPair[] weaponActionPairs;
    private Coroutine switchCoroutine;

    [Header("Earth")]
    private EarthPrimaryAction earthPrimaryAction;
    private Rock currRock;
    private Coroutine rockSummonCoroutine;

    [Header("Flamethrower")]
    private FireSecondaryAction fireSecondaryAction;

    [Header("Death")]
    private bool isDead; // to deal with death delay

    [Header("Health")]
    private List<WeaponActionPair> deathSubscriptions; // for unsubscribing later

    private new void Awake() {

        base.Awake();

        // set up mechanic statuses early so scripts can change them earlier too
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

            itemSelector.SetWeaponData(action.GetWeaponData(), i); // add weapon item to hotbar

            if (primaryAction)
                health.OnDeath += primaryAction.OnDeath; // subscribe to death event

            if (secondaryAction)
                health.OnDeath += secondaryAction.OnDeath; // subscribe to death event

            deathSubscriptions.Add(action); // add to list for unsubscribing later

        }
    }

    private new void Start() {

        base.Start();

        earthPrimaryAction = GetComponent<EarthPrimaryAction>();

        fireSecondaryAction = GetComponent<FireSecondaryAction>();

        currWeapon = weaponActionPairs[0].GetWeapon(); // get first weapon
        charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon to first weapon by default

    }

    private void Update() {

        if (isDead)
            return; // player is dead, no need to update

        #region ACTIONS

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
        if (currSecondaryAction) { // make sure slot has a weapon/secondary action in it

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

        #endregion

        #region WEAPON SWITCHING

        /* SCROLL WHEEL WEAPON SWITCHING */
        if (Input.mouseScrollDelta.y > 0f && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.CycleSlot(-1); // cycle hotbar slot backwards

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.mouseScrollDelta.y < 0f && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.CycleSlot(1); // cycle hotbar slot forwards

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        }

        /* KEY WEAPON SWITCHING */
        if (Input.GetKeyDown(KeyCode.Alpha1) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(0);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha2) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(1);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha3) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(2);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha4) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(3);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha5) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(4);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha6) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(5);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha7) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(6);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha8) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(7);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha9) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(8);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha0) && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            itemSelector.SelectSlot(9);

            if (currPrimaryAction) // make sure primary action exists
                currPrimaryAction.enabled = false; // disable current primary action

            if (currSecondaryAction) // make sure secondary action exists
                currSecondaryAction.enabled = false; // disable current secondary action

            // set new weapon and actions
            if (itemSelector.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon(); // set new weapon
                currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction();
                currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction();

                // placed here to make sure weapon exists before starting cooldown
                if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
                switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

                charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currPrimaryAction) { // make sure primary action exists

                    currPrimaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetPrimaryAction(); // update primary action

                    if (currPrimaryAction) // check if new primary action exists
                        currPrimaryAction.enabled = true; // enable new action

                }

                if (currSecondaryAction) { // make sure secondary action exists

                    currSecondaryAction = weaponActionPairs[itemSelector.GetCurrWeapon()].GetSecondaryAction(); // update secondary action

                    if (currSecondaryAction) // check if new secondary action exists
                        currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeaponHandler.ChangeWeapon(null, null); // remove weapon

            }
        }

        #endregion

    }

    private new void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water"))  // barrier can save player from water
            health.Kill();

    }

    private new void OnDisable() {

        base.OnDisable();

        // unsubscribe from all events
        foreach (WeaponActionPair action in deathSubscriptions) {

            if (action.GetPrimaryAction())
                health.OnDeath -= action.GetPrimaryAction().OnDeath;

            if (action.GetSecondaryAction())
                health.OnDeath -= action.GetSecondaryAction().OnDeath;

        }
    }

    #region EARTH

    public Rock OnSummonRock(EarthPrimaryAction action, Rock rockPrefab, float maxThrowDuration) {

        rockSummonCoroutine = StartCoroutine(HandleSummonRock(action, rockPrefab, maxThrowDuration));
        return currRock;

    }

    private IEnumerator HandleSummonRock(EarthPrimaryAction action, Rock rockPrefab, float maxThrowDuration) {

        DisableAllMechanics(); // disable all mechanics while rock is being summoned (except primary action)
        EnableMechanic(MechanicType.PrimaryAction); // enable only primary action while rock is summoned
        DisableCoreScripts(); // disable all scripts while rock is being summoned (including weapon handler)

        currRock = Instantiate(rockPrefab, transform.position, Quaternion.identity); // instantiate rock (will play summon animation & rotate itself automatically)
        anim.SetBool("isSummoningRock", true); // play rock summon animation

        yield return null; // wait for animation to start
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length); // wait for animation to end (so rock can be dropped during animation because this coroutine won't be null)

        SetWeaponHandlerEnabled(true); // enable weapon handler when rock is fully summoned
        action.OnThrowReady(); // trigger throw ready event

        rockSummonCoroutine = null;

    }

    public void OnRockThrow() => StartCoroutine(HandleRockThrow()); // handle rock throw

    private IEnumerator HandleRockThrow() {

        // must wait for two frames to allow shot to be fired
        yield return null;
        yield return null;

        charWeaponHandler.ShootStop(); // stop shooting weapon (to deal with infinite shooting bug | do this before disabling the core scripts)
        EnableCoreScripts(); // enable all scripts after rock is dropped/thrown (except weapon handler | don't want player to use weapon unless rock is fully summoned)
        SetWeaponHandlerEnabled(false); // disable weapon handler when rock is thrown
        EnableAllMechanics(); // enable all mechanics after rock is dropped/thrown

    }

    public void OnDestroyRock(bool activateMechanics) {

        if (rockSummonCoroutine != null) StopCoroutine(rockSummonCoroutine); // stop rock summon coroutine if it's running

        Destroy(currRock.gameObject); // destroy rock

        corgiController.SetForce(Vector2.zero); // reset player movement
        anim.SetBool("isSummoningRock", false);

        if (activateMechanics) {

            EnableCoreScripts(); // enable all scripts after rock is dropped/thrown (except weapon handler | don't want player to use weapon unless rock is fully summoned)
            SetWeaponHandlerEnabled(false); // disable weapon handler when rock is dropped
            EnableAllMechanics(); // enable all mechanics after rock is dropped/thrown

        }
    }

    #endregion

    #region UTILITIES

    public Weapon GetCurrentWeapon() => weaponActionPairs[itemSelector.GetCurrWeapon()].GetWeapon();

    private IEnumerator HandleSwitchCooldown() {

        charWeaponHandler.AbilityPermitted = false; // disable ability use
        yield return new WaitForSeconds(weaponActionPairs[itemSelector.GetCurrWeapon()].GetSwitchCooldown()); // wait for switch cooldown
        charWeaponHandler.AbilityPermitted = true; // enable ability use

    }

    protected override void OnDeath() {

        base.OnDeath();
        isDead = true;

    }

    protected override void OnRespawn() {

        base.OnRespawn();
        isDead = false;

    }

    public Vector2 GetDirectionRight() => character.IsFacingRight ? transform.right : -transform.right;

    public bool IsGrounded() => corgiController.State.IsGrounded;

    public Animator GetAnimator() => anim;

    #endregion

}
