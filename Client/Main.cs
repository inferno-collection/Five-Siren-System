// Five Siren System
// Copyright (c) 2019-2023, Christopher M, Inferno Collection. All rights reserved.
// This project is licensed under the following:
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to use, copy, modify, and merge the software, under the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// The software may not be sold in any format.
// Modified copies of the software may only be shared in an uncompiled format.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using InfernoCollection.Siren.Client.Models;

namespace InfernoCollection.Siren.Client
{
    public class Main : ClientScript
    {
        #region Variables

        internal bool
            _5ssEnabled = true,
            _indicatorActive = false,
            _muteDefaultSiren = true,
            _muteSirenTemp = false,
            _shouldDeactiveIndicator = false;

        internal SirenState _sirenToneTemp = SirenState.Off;

        internal int _lastLivery = 0;

        internal readonly Dictionary<Vehicle, VehicleSiren> _vehicleSirens = new Dictionary<Vehicle, VehicleSiren>();

        #endregion

        #region Configuration

        internal float
            _airHornVolume = 0.3f,
            _lightCycleVolume = 0.3f,
            _sirenCycleVolume = 0.3f;

        internal readonly IReadOnlyList<SirenController> _sirenControllers = new List<SirenController>()
        {
            /*
            RESIDENT_VEHICLES_SIREN_FIRETRUCK_QUICK_01 // SIREN_FIRETRUCK_QUICK_01
            RESIDENT_VEHICLES_SIREN_FIRETRUCK_WAIL_01 // SIREN_FIRETRUCK_WAIL_01
            RESIDENT_VEHICLES_SIREN_QUICK_01 // SIREN_QUICK_01
            RESIDENT_VEHICLES_SIREN_QUICK_02 // SIREN_QUICK_02
            RESIDENT_VEHICLES_SIREN_QUICK_03 // SIREN_QUICK_03
            RESIDENT_VEHICLES_SIREN_WAIL_01 // SIREN_WAIL_01
            RESIDENT_VEHICLES_SIREN_WAIL_02 // SIREN_WAIL_02
            RESIDENT_VEHICLES_SIREN_WAIL_03 // SIREN_WAIL_03

            VEHICLES_HORNS_AMBULANCE_WARNING // AMBULANCE_WARNING
            VEHICLES_HORNS_FIRETRUCK_WARNING // FIRE_TRUCK_HORN
            VEHICLES_HORNS_POLICE_WARNING // POLICE_WARNING
            VEHICLES_HORNS_SIREN_1 // SIREN_PA20A_WAIL
            VEHICLES_HORNS_SIREN_2 // SIREN_2
            SIRENS_AIRHORN // AIRHORN_EQD
            */

            new SirenController()
            {
                Prefix = new string[1] { "pd" },
                Primary = "VEHICLES_HORNS_SIREN_1",
                Secondary = "VEHICLES_HORNS_SIREN_2"
            },

            new SirenController()
            {
                Prefix = new string[2] { "so", "lc" },
                Primary = "RESIDENT_VEHICLES_SIREN_WAIL_03",
                Secondary = "RESIDENT_VEHICLES_SIREN_QUICK_03"
            },

            new SirenController()
            {
                Prefix = new string[2] { "hp", "dp" },
                Primary = "RESIDENT_VEHICLES_SIREN_WAIL_02",
                Secondary = "RESIDENT_VEHICLES_SIREN_QUICK_02"
            },

            // "weewooweeewoo" and "bwommmp bwommmp"
            new SirenController()
            {
                SpecificModels = new string[1] { "fd2" },
                Primary = "RESIDENT_VEHICLES_SIREN_WAIL_01",
                Secondary = "RESIDENT_VEHICLES_SIREN_QUICK_01",
                Tertiary = "VEHICLES_HORNS_AMBULANCE_WARNING",
                Horn = "VEHICLES_HORNS_FIRETRUCK_WARNING",
                HornOverlap = true,
                Powercall = "VEHICLES_HORNS_AMBULANCE_WARNING"
            },

            // "wooooooreeeeeeeeeeeewoooo" and "bwommmp bwoomp"
            new SirenController()
            {
                SpecificModels = new string[3] { "fd1", "fd4", "fd5" },
                Primary = "RESIDENT_VEHICLES_SIREN_FIRETRUCK_WAIL_01",
                Secondary = "RESIDENT_VEHICLES_SIREN_FIRETRUCK_QUICK_01",
                Tertiary = "VEHICLES_HORNS_AMBULANCE_WARNING",
                Horn = "VEHICLES_HORNS_FIRETRUCK_WARNING",
                HornOverlap = true,
                Powercall = "VEHICLES_HORNS_AMBULANCE_WARNING"
            },

            // "weewoooweeewooo" and "meep meep"
            new SirenController()
            {
                SpecificModels = new string[7] { "fd3", "fd6", "fd7", "fd8", "fd9", "fd10", "fddinghy" },
                Primary = "RESIDENT_VEHICLES_SIREN_WAIL_01",
                Secondary = "RESIDENT_VEHICLES_SIREN_QUICK_01",
                Tertiary = "VEHICLES_HORNS_AMBULANCE_WARNING"
            }
        };

        internal readonly List<VehicleClass> _ignoredClasses = new List<VehicleClass> { VehicleClass.Boats, VehicleClass.Helicopters, VehicleClass.Planes, VehicleClass.Trains };

        internal readonly IReadOnlyDictionary<string, int[]> _vehicleLightStage = new Dictionary<string, int[]>
        {
            { "pd1", new int[1] { 1 } },
            { "pd23", new int[1] { 1 } },
            { "pd24", new int[1] { 1 } },
            { "pd25", new int[1] { 1 } },
            { "pd31", new int[1] { 10 } },
            { "pd32", new int[1] { 10 } },
            { "pd33", new int[1] { 10 } },
            { "pd35", new int[1] { 9 } },
            { "fd6", new int[1] { 1 } },
            { "fd7", new int[1] { 1 } },
            { "fd9", new int[1] { 8 } },
            { "dps1", new int[3] { 10, 11, 12} },
            { "dps2", new int[3] { 10, 11, 12} },
            { "dps3", new int[3] { 10, 11, 12} },
            { "dps4", new int[3] { 10, 11, 12} },
            { "dps5", new int[3] { 10, 11, 12} },
        };

        internal readonly IReadOnlyDictionary<string, bool> _whitelistedSirens = new Dictionary<string, bool>
        {
            // LSPD subdivisions
            { "atvparks", true },
            { "atvport", true },
            { "portski", true },
            // LEO misc
            { "rbs", true },
            { "leodinghy", true },
            { "defender", true },
            { "fddinghy", true },
            // LSFD
            { "fdatv", true }
        };

        #endregion

        #region Tick Handlers

        [Tick]
        internal async Task CleanupSounds()
        {
            foreach (KeyValuePair<Vehicle, VehicleSiren> veh in _vehicleSirens.ToArray())
            {
                Vehicle vehicle = veh.Key;
                VehicleSiren vehicleSiren = veh.Value;

                bool isVehRemoved = !Entity.Exists(vehicle) || vehicle.IsDead;

                if ((vehicleSiren.SirenState == SirenState.Off || isVehRemoved) && vehicleSiren.SirenSoundId > -1)
                {
                    API.StopSound(vehicleSiren.SirenSoundId);
                    API.ReleaseSoundId(vehicleSiren.SirenSoundId);
                    _vehicleSirens[vehicle].SirenSoundId = -1;
                    _vehicleSirens[vehicle].SirenState = SirenState.Off;
                }

                if ((vehicleSiren.PowercallState == PowercallState.Off || isVehRemoved) && vehicleSiren.PowercallSoundId > -1)
                {
                    API.StopSound(vehicleSiren.PowercallSoundId);
                    API.ReleaseSoundId(vehicleSiren.PowercallSoundId);
                    _vehicleSirens[vehicle].PowercallSoundId = -1;
                    _vehicleSirens[vehicle].PowercallState = PowercallState.Off;
                }

                if ((vehicleSiren.ManualSirenState == ManualSirenState.Off || isVehRemoved || vehicle.IsSeatFree(VehicleSeat.Driver)) && vehicleSiren.ManualSirenSoundId > -1)
                {
                    API.StopSound(vehicleSiren.ManualSirenSoundId);
                    API.ReleaseSoundId(vehicleSiren.ManualSirenSoundId);
                    _vehicleSirens[vehicle].ManualSirenSoundId = -1;
                    _vehicleSirens[vehicle].ManualSirenState = ManualSirenState.Off;
                }

                if (isVehRemoved)
                {
                    _vehicleSirens.Remove(vehicle);
                }
                else if (_vehicleSirens[vehicle].SirenController == null || _lastLivery != vehicle.Mods.Livery)
                {
                    _lastLivery = vehicle.Mods.Livery;

                    _vehicleSirens[vehicle].SirenController = GetSirenController(vehicle);
                }
            }

            await Delay(200);
        }

        [Tick]
        internal async Task SirenTask()
        {
            API.DistantCopCarSirens(false);
            API.StartAudioScene("CHARACTER_CHANGE_IN_SKY_SCENE");

            Ped playerPed = Game.PlayerPed;
            Vehicle playerVeh = playerPed.LastVehicle;

            if (!Entity.Exists(playerVeh) || playerVeh.Driver != playerPed)
            {
                await Delay(0);
                return;
            }

            VehicleSiren vehSiren = _vehicleSirens.Get(playerVeh);

            if (vehSiren.SirenController == null)
            {
                vehSiren.SirenController = GetSirenController(playerVeh);
            }

            bool manualSiren = false;
            bool manualAirHorn = false;

            Game.DisableControlThisFrame(0, Control.VehicleNextRadioTrack);
            Game.DisableControlThisFrame(0, Control.VehiclePrevRadioTrack);

            if (playerVeh.ClassType == VehicleClass.Emergency || _whitelistedSirens.ContainsKey(playerVeh.DisplayName.ToLower()))
            {
                Game.DisableControlThisFrame(0, Control.CharacterWheel);
                Game.DisableControlThisFrame(0, Control.VehicleHorn);
                if (HasSiren(playerVeh))
                {
                    Game.DisableControlThisFrame(0, Control.VehicleCinCam);
                    Game.DisableControlThisFrame(0, Control.VehicleNextRadio);
                    Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);
                }

                playerVeh.RadioStation = RadioStation.RadioOff;
                playerVeh.IsRadioEnabled = false;

                ToggleVehicleMuteDefaultSiren(playerVeh, !vehSiren.SirenController.HornOverlap || vehSiren.SirenState != SirenState.Wail);

                if (!playerVeh.IsSirenActive && vehSiren.SirenState > SirenState.Off)
                {
                    SetVehicleSirenState(playerVeh, SirenState.Off);
                }

                if (!playerVeh.IsSirenActive && vehSiren.PowercallState == PowercallState.On)
                {
                    ToggleVehiclePowercallState(playerVeh, PowercallState.Off);
                }

                if (!Game.IsPaused && Entity.Exists(playerPed.CurrentVehicle))
                {
                    if (Game.IsDisabledControlJustPressed(0, Control.VehicleRadioWheel))
                    {
                        CreateElsClick(playerVeh.IsSirenActive ? "Off" : "On", _lightCycleVolume);
                        playerVeh.IsSirenActive = !playerVeh.IsSirenActive;
                    }
                    else if (Game.IsDisabledControlJustPressed(0, Control.CharacterWheel) && HasSiren(playerVeh))
                    {
                        if (vehSiren.SirenState == SirenState.Off)
                        {
                            if (playerVeh.IsSirenActive)
                            {
                                CreateElsClick("Upgrade", _sirenCycleVolume);
                                SetVehicleSirenState(playerVeh, SirenState.Wail);
                            }
                        }
                        else
                        {
                            CreateElsClick("Downgrade", _sirenCycleVolume);
                            SetVehicleSirenState(playerVeh, SirenState.Off);
                        }
                    }
                    else if (Game.IsControlJustPressed(0, Control.PhoneUp) && HasSiren(playerVeh))
                    {
                        if (vehSiren.PowercallState == PowercallState.On)
                        {
                            CreateElsClick("Downgrade", _sirenCycleVolume);
                            ToggleVehiclePowercallState(playerVeh, PowercallState.Off);
                        }
                        else if (playerVeh.IsSirenActive)
                        {
                            CreateElsClick("Upgrade", _sirenCycleVolume);
                            ToggleVehiclePowercallState(playerVeh, PowercallState.On);
                        }
                    }

                    if (vehSiren.SirenState > SirenState.Off && (Game.IsDisabledControlJustReleased(0, Control.VehicleCinCam) || Game.IsDisabledControlJustReleased(0, Control.VehicleNextRadio)) && playerVeh.IsSirenActive && HasSiren(playerVeh))
                    {
                        CreateElsClick("Upgrade", _sirenCycleVolume);
                        SetVehicleSirenState(playerVeh, vehSiren.SirenState == SirenState.Piercer ? SirenState.Wail : vehSiren.SirenState + 1);
                    }

                    if (vehSiren.SirenState == SirenState.Off)
                    {
                        if ((Game.IsDisabledControlJustPressed(0, Control.VehicleCinCam) || Game.IsDisabledControlJustPressed(0, Control.VehicleNextRadio)) && HasSiren(playerVeh))
                        {
                            CreateElsClick("Upgrade", _sirenCycleVolume);
                        }

                        manualSiren = HasSiren(playerVeh) && (Game.IsDisabledControlPressed(0, Control.VehicleCinCam) || Game.IsDisabledControlPressed(0, Control.VehicleNextRadio));
                    }

                    if (!playerPed.IsDead)
                    {
                        if (Game.IsDisabledControlJustPressed(0, Control.VehicleHorn) && HasSiren(playerVeh))
                        {
                            CreateElsClick("Upgrade", _airHornVolume);
                        }

                        manualAirHorn = Game.IsDisabledControlPressed(0, Control.VehicleHorn) && HasSiren(playerVeh);

                        if (Game.IsDisabledControlPressed(0, Control.VehicleHorn) && !HasSiren(playerVeh))
                        {
                            playerVeh.SoundHorn(1);
                        }
                    }

                    ManualSirenState newManualState = ManualSirenState.Off;

                    if (manualAirHorn && !manualSiren)
                    {
                        newManualState = ManualSirenState.On;
                    }
                    else if (!manualAirHorn && manualSiren)
                    {
                        newManualState = ManualSirenState.Wail;
                    }
                    else if (manualAirHorn && manualSiren)
                    {
                        newManualState = ManualSirenState.Yelp;
                    }

                    if (newManualState > ManualSirenState.Off)
                    {
                        if (!vehSiren.SirenController.HornOverlap && vehSiren.SirenState > SirenState.Off && !_muteSirenTemp)
                        {
                            _sirenToneTemp = vehSiren.SirenState;
                            SetVehicleSirenState(playerVeh, SirenState.Off);
                            _muteSirenTemp = true;
                        }
                    }
                    else if (!vehSiren.SirenController.HornOverlap && _muteSirenTemp)
                    {
                        SetVehicleSirenState(playerVeh, _sirenToneTemp);
                        _muteSirenTemp = false;
                    }

                    if (vehSiren.ManualSirenState != newManualState)
                    {
                        SetVehicleManualSirenState(playerVeh, newManualState);
                    }
                }
            }

            if (!_ignoredClasses.Contains(playerVeh.ClassType) && !Game.IsPaused && Entity.Exists(playerPed.CurrentVehicle))
            {
                if (Game.IsDisabledControlJustPressed(0, Control.VehiclePrevRadioTrack))
                {
                    PlayManagedSoundFrontend(vehSiren.IndicatorState == IndicatorState.Left ? "NAV_UP_DOWN" : "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                    vehSiren.IndicatorState = vehSiren.IndicatorState == IndicatorState.Left ? IndicatorState.Off : IndicatorState.Left;

                    ToggleVehicleIndicatorState(playerVeh, vehSiren.IndicatorState);
                }
                else if (Game.IsDisabledControlJustPressed(0, Control.VehicleNextRadioTrack))
                {
                    PlayManagedSoundFrontend(vehSiren.IndicatorState == IndicatorState.Right ? "NAV_UP_DOWN" : "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                    vehSiren.IndicatorState = vehSiren.IndicatorState == IndicatorState.Right ? IndicatorState.Off : IndicatorState.Right;

                    ToggleVehicleIndicatorState(playerVeh, vehSiren.IndicatorState);
                }
                else if (Game.IsControlJustPressed(0, Control.FrontendRright))
                {
                    PlayManagedSoundFrontend(vehSiren.IndicatorState == IndicatorState.Hazard ? "NAV_UP_DOWN" : "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                    vehSiren.IndicatorState = vehSiren.IndicatorState == IndicatorState.Hazard ? IndicatorState.Off : IndicatorState.Hazard;

                    ToggleVehicleIndicatorState(playerVeh, vehSiren.IndicatorState);
                }
            }

            await Task.FromResult(0);
        }

        [Tick]
        internal async Task SyncControl()
        {
            Ped playerPed = Game.PlayerPed;
            Vehicle playerVeh = playerPed.LastVehicle;

            if (!Entity.Exists(playerVeh) || (Entity.Exists(playerVeh.Driver) && playerVeh.Driver != playerPed))
            {
                return;
            }

            VehicleSiren vehSiren = _vehicleSirens.Get(playerVeh);

            if (vehSiren.SirenController == null)
            {
                await Delay(100);
                return;
            }

            if (vehSiren.IndicatorState == IndicatorState.Left || vehSiren.IndicatorState == IndicatorState.Right)
            {
                if (_shouldDeactiveIndicator && Math.Abs(playerVeh.SteeringAngle) < 20f)
                {
                _shouldDeactiveIndicator = false;
                vehSiren.IndicatorState = IndicatorState.Off; 
                ToggleVehicleIndicatorState(playerVeh, IndicatorState.Off);
                }
                else
                {
                    if ((vehSiren.IndicatorState == IndicatorState.Left && playerVeh.SteeringAngle > 20f) || (vehSiren.IndicatorState == IndicatorState.Right && playerVeh.SteeringAngle < -20f))
                    {
                        _shouldDeactiveIndicator = true;
                    }
                }
            }

            if (!_ignoredClasses.Contains(playerVeh.ClassType) || _whitelistedSirens.ContainsKey(playerVeh.DisplayName.ToLower()))
            {
                if (playerVeh.ClassType == VehicleClass.Emergency || _whitelistedSirens.ContainsKey(playerVeh.DisplayName.ToLower()))
                {
                    bool defaultSirenMuted = !vehSiren.SirenController.HornOverlap || vehSiren.SirenState != SirenState.Wail;
                    TriggerServerEvent("Inferno-Collection:Server:5SS:UpdateCache", (int)vehSiren.IndicatorState, defaultSirenMuted, (int)vehSiren.SirenState, (int)vehSiren.PowercallState, (int)vehSiren.ManualSirenState);
                }
                else
                {
                    TriggerServerEvent("Inferno-Collection:Server:5SS:UpdateCache", (int)vehSiren.IndicatorState);
                }
            }
            
            await Delay(500);
        }

        [Tick]
        internal async Task SirenEjectoStop()
        {
            Ped playerPed = Game.PlayerPed;
            Vehicle currVeh = playerPed.CurrentVehicle;
            Vehicle lastVeh = playerPed.LastVehicle;

            if (!Entity.Exists(lastVeh) || !_vehicleSirens.ContainsKey(lastVeh))
            {
                await Delay(100);
                return;
            }

            bool inVehicle = Entity.Exists(currVeh);

            if (inVehicle && currVeh.Driver == playerPed)
            {
                _vehicleSirens[currVeh].LastOccupied = DateTime.UtcNow;
            } 
            else if (_vehicleSirens[lastVeh].LastOccupied != DateTime.MinValue && _vehicleSirens[lastVeh].LastOccupied.AddSeconds(2) > DateTime.UtcNow && lastVeh.Speed < 1)
            {
                VehicleSiren vehSiren = _vehicleSirens[lastVeh];

                if (vehSiren.SirenState > SirenState.Off)
                {
                    SetVehicleSirenState(lastVeh, SirenState.Off);
                }

                if (vehSiren.ManualSirenState > ManualSirenState.Off)
                {
                    SetVehicleManualSirenState(lastVeh, ManualSirenState.Off);
                }

                if (vehSiren.PowercallState > PowercallState.Off)
                {
                    ToggleVehiclePowercallState(lastVeh, PowercallState.Off);
                }
            }

            await Delay(100);
        }

        [Tick]
        internal async Task LightStage()
        {
            if (!Game.IsDisabledControlJustPressed(0, Control.ReplayCameraUp))
            {
                return;
            }

            Vehicle currentVehicle = Game.PlayerPed?.CurrentVehicle;

            if (Entity.Exists(currentVehicle) && currentVehicle.Driver == Game.PlayerPed && _vehicleLightStage.ContainsKey(currentVehicle.DisplayName.ToLower()))
            {
                bool state = currentVehicle.IsExtraOn(_vehicleLightStage[currentVehicle.DisplayName.ToLower()][0]);
                CreateElsClick(!state ? "Upgrade" : "Downgrade", _sirenCycleVolume);
                _vehicleLightStage[currentVehicle.DisplayName.ToLower()].ToList().ForEach(e => currentVehicle.ToggleExtra(e, !state));
                await Delay(500);
            }

            await Task.FromResult(0);
        }

        #endregion

        #region Event Handlers
        [EventHandler("Inferno-Collection:Client:5SS:Toggle")]
        internal void OnToggle5SS(bool toggle)
        {
            _5ssEnabled = toggle;

            Ped playerPed = Game.PlayerPed;
            Vehicle playerVeh = playerPed.LastVehicle;

            if (!Entity.Exists(playerVeh) || playerVeh.Driver != playerPed)
            {
                return;
            }

            VehicleSiren vehSiren = _vehicleSirens.Get(playerVeh);

            vehSiren.SirenController = GetSirenController(playerVeh);
        }

        [EventHandler("Inferno-Collection:Client:5SS:OnSirenUpdated")]
        internal void OnServerSirenUpdated(int src, int indicatorState, bool defSirenMuted, int sirenState, int powercallState, int manSirenState)
        {
            Ped playerPed = Players[src]?.Character;

            if (Entity.Exists(playerPed) && !playerPed.IsDead && playerPed != LocalPlayer.Character && Entity.Exists(playerPed.LastVehicle))
            {
                ToggleVehicleIndicatorState(playerPed.LastVehicle, (IndicatorState)indicatorState);

                if (playerPed.LastVehicle.ClassType == VehicleClass.Emergency || _whitelistedSirens.ContainsKey(playerPed.LastVehicle.DisplayName.ToLower()))
                {
                    ToggleVehicleMuteDefaultSiren(playerPed.LastVehicle, defSirenMuted);
                    ToggleVehiclePowercallState(playerPed.LastVehicle, (PowercallState)powercallState);

                    SetVehicleSirenState(playerPed.LastVehicle, (SirenState)sirenState);
                    SetVehicleManualSirenState(playerPed.LastVehicle, (ManualSirenState)manSirenState);
                }
            }
        }

        [EventHandler("Inferno-Collection:Client:ELSClick")]
        internal void CreateElsClick(string soundFile, float soundVolume) => 
            API.SendNuiMessage(string.Format("{{\"transactionType\":\"playSound\",\"transactionFile\":\"{0}\",\"transactionVolume\":\"{1}\"}}", soundFile, soundVolume));

        [EventHandler("Inferno-Collection:Client:Siren:volume:airHornClickVolume")]
        internal void OnSetAirHornVolume(float volume) => _airHornVolume = volume;

        [EventHandler("Inferno-Collection:Client:Siren:volume:lightingClickVolume")]
        internal void OnSetLightingClickVolume(float volume) => _lightCycleVolume = volume;

        [EventHandler("Inferno-Collection:Client:Siren:volume:sirenClickVolume")]
        internal void OnSetSirenClickVolume(float volume) => _sirenCycleVolume = volume;

        #endregion

        #region ELS Control Functions

        internal void ToggleVehicleIndicatorState(Vehicle veh, IndicatorState newState)
        {
            if (Entity.Exists(veh) && !veh.IsDead)
            {
                veh.IsLeftIndicatorLightOn = newState == IndicatorState.Left || newState == IndicatorState.Hazard;
                veh.IsRightIndicatorLightOn = newState == IndicatorState.Right || newState == IndicatorState.Hazard;

                _vehicleSirens.Get(veh).IndicatorState = newState;
            }
        }

        internal void ToggleVehicleMuteDefaultSiren(Vehicle veh, bool isMuted)
        {
            if (Entity.Exists(veh) && !veh.IsDead)
            {
                API.SetVehicleHasMutedSirens(veh.Handle, isMuted);
            }
        }

        internal void ToggleVehiclePowercallState(Vehicle veh, PowercallState newState)
        {
            VehicleSiren vehSiren = _vehicleSirens.Get(veh);

            if (Entity.Exists(veh) && !veh.IsDead && vehSiren.PowercallState != newState)
            {
                if (vehSiren.PowercallSoundId >= -1)
                {
                    API.StopSound(vehSiren.PowercallSoundId);
                    API.ReleaseSoundId(vehSiren.PowercallSoundId);
                    vehSiren.PowercallSoundId = -1;
                }

                if (vehSiren.SirenController == null)
                {
                    return;
                }

                if (newState == PowercallState.On)
                {
                    vehSiren.PowercallSoundId = API.GetSoundId();

                    API.PlaySoundFromEntity(vehSiren.PowercallSoundId, vehSiren.SirenController.Powercall ?? vehSiren.SirenController.Primary, veh.Handle, null, false, 0);
                }

                vehSiren.PowercallState = newState;
            }
        }

        internal void SetVehicleSirenState(Vehicle veh, SirenState newState)
        {
            VehicleSiren vehSiren = _vehicleSirens.Get(veh);

            if (Entity.Exists(veh) && !veh.IsDead && vehSiren.SirenState != newState)
            {
                if (vehSiren.SirenSoundId >= -1)
                {
                    API.StopSound(vehSiren.SirenSoundId);
                    API.ReleaseSoundId(vehSiren.SirenSoundId);
                    vehSiren.SirenSoundId = -1;
                }

                if (vehSiren.SirenController == null)
                {
                    return;
                }

                ToggleVehicleMuteDefaultSiren(veh, newState != SirenState.Wail || !vehSiren.SirenController.HornOverlap);

                if (newState != SirenState.Off)
                {
                    vehSiren.SirenSoundId = API.GetSoundId();

                    if (newState == SirenState.Wail)
                    {
                        if (!vehSiren.SirenController.HornOverlap)
                        {
                            API.PlaySoundFromEntity(vehSiren.SirenSoundId, vehSiren.SirenController.Primary, veh.Handle, null, false, 0);
                        }
                    }
                    else if (newState == SirenState.Yelp)
                    {
                        API.PlaySoundFromEntity(vehSiren.SirenSoundId, vehSiren.SirenController.Secondary, veh.Handle, null, false, 0);
                    }
                    else if (newState == SirenState.Piercer)
                    {
                        API.PlaySoundFromEntity(vehSiren.SirenSoundId, vehSiren.SirenController.Tertiary, veh.Handle, null, false, 0);
                    }
                }

                vehSiren.SirenState = newState;
            }
        }

        internal void SetVehicleManualSirenState(Vehicle veh, ManualSirenState newState)
        {
            VehicleSiren vehSiren = _vehicleSirens.Get(veh);

            if (Entity.Exists(veh) && !veh.IsDead && vehSiren.ManualSirenState != newState)
            {
                if (vehSiren.ManualSirenSoundId >= -1)
                {
                    API.StopSound(vehSiren.ManualSirenSoundId);
                    API.ReleaseSoundId(vehSiren.ManualSirenSoundId);
                    vehSiren.ManualSirenSoundId = -1;
                }

                if (vehSiren.SirenController == null)
                {
                    return;
                }

                if (newState != ManualSirenState.Off)
                {
                    vehSiren.ManualSirenSoundId = API.GetSoundId();

                    if (newState == ManualSirenState.On)
                    {
                        API.PlaySoundFromEntity(vehSiren.ManualSirenSoundId, vehSiren.SirenController.Horn, veh.Handle, null, false, 0);
                    }
                    else if (newState == ManualSirenState.Wail)
                    {
                        API.PlaySoundFromEntity(vehSiren.ManualSirenSoundId, vehSiren.SirenController.Primary, veh.Handle, null, false, 0);
                    }
                    else if (newState == ManualSirenState.Yelp)
                    {
                        API.PlaySoundFromEntity(vehSiren.ManualSirenSoundId, vehSiren.SirenController.Secondary, veh.Handle, null, false, 0);
                    }
                }

                vehSiren.ManualSirenState = newState;
            }
        }

        #endregion

        #region Misc. Functions
        internal bool HasSiren(Vehicle veh) => veh.ClassType == VehicleClass.Emergency || (_whitelistedSirens.ContainsKey(veh.DisplayName.ToLower()) && _whitelistedSirens[veh.DisplayName.ToLower()]);

        internal async void PlayManagedSoundFrontend(string soundName, string soundSet = null)
        {
            int soundId = Audio.PlaySoundFrontend(soundName, soundSet);

            while (!Audio.HasSoundFinished(soundId))
            {
                await Delay(200);
            }

            Audio.ReleaseSound(soundId);
        }

        internal SirenController GetSirenController(Vehicle veh)
        {
            string
                vehicleName = veh.DisplayName.ToLower(),
                vehiclePrefix = vehicleName.Substring(0, 2);
            SirenController sirenController;

            if (!_5ssEnabled)
            {
                // If using a fire vehicle, pass correct siren strings
                if (vehiclePrefix == "fd")
                {
                    return _sirenControllers.FirstOrDefault(i => i.SpecificModels != null && i.SpecificModels.Contains(vehicleName));
                }

                return new SirenController() { Primary = "VEHICLES_HORNS_SIREN_1", Secondary = "VEHICLES_HORNS_SIREN_2" };
            }

            int
                vehicleLivery = veh.Mods.Livery,
                vehicleNumber = vehicleName.Last() - '0';

            sirenController = _sirenControllers
                .FirstOrDefault(i => i.SpecificModels != null && i.SpecificModels.Contains(vehicleName));

            if (sirenController == null)
            {
                sirenController = _sirenControllers.FirstOrDefault(i => i.Prefix != null && i.Prefix.Contains(vehiclePrefix));
            }

            if (sirenController == null)
            {
                #region Slicktop
                if (vehiclePrefix == "st")
                {
                    switch (vehicleNumber)
                    {
                        case 1:
                            switch (vehicleLivery)
                            {
                                case 4:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("hp"));
                                    break;

                                case 5:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("so"));
                                    break;

                                default:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("pd"));
                                    break;
                            }
                            break;

                        default:
                            switch (vehicleLivery)
                            {
                                case 3:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("hp"));
                                    break;

                                case 4:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("so"));
                                    break;

                                default:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("pd"));
                                    break;
                            }
                            break;
                    }
                }
                #endregion

                #region Bikes
                else if (vehicleName == "bike1" || vehicleName == "bike2")
                {
                    switch (vehicleNumber)
                    {
                        case 1:
                            switch (vehicleLivery)
                            {
                                case 1:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("hp"));
                                    break;

                                case 2:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("dp"));
                                    break;

                                case 3:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("so"));
                                    break;

                                default:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("pd"));
                                    break;
                            }
                            break;

                        case 2:
                            switch (vehicleLivery)
                            {
                                case 1:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("hp"));
                                    break;

                                case 2:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("so"));
                                    break;

                                case 3:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("dp"));
                                    break;

                                default:
                                    sirenController = _sirenControllers.First(i => i.Prefix.Contains("pd"));
                                    break;
                            }
                            break;
                    }
                }
                #endregion

                #region Boats
                else if (vehicleName == "defender" || vehicleName == "rbs")
                {
                    switch (vehicleLivery)
                    {
                        case 1:
                            sirenController = _sirenControllers.First(i => i.Prefix.Contains("hp"));
                            break;

                        case 2:
                            sirenController = _sirenControllers.First(i => i.Prefix.Contains("so"));
                            break;

                        case 3:
                            sirenController = _sirenControllers.First(i => i.SpecificModels != null && i.SpecificModels.Contains("fddinghy"));
                            break;

                        default:
                            sirenController = _sirenControllers.First(i => i.Prefix.Contains("pd"));
                            break;
                    }
                }
                else if (vehicleName == "leodinghy")
                {
                    switch (vehicleLivery)
                    {
                        case 1:
                            sirenController = _sirenControllers.First(i => i.Prefix.Contains("hp"));
                            break;

                        case 2:
                            sirenController = _sirenControllers.First(i => i.Prefix.Contains("so"));
                            break;

                        default:
                            sirenController = _sirenControllers.First(i => i.Prefix.Contains("pd"));
                            break;
                    }
                }
                #endregion
            }

            return sirenController ?? new SirenController() { Primary = "VEHICLES_HORNS_SIREN_1", Secondary = "VEHICLES_HORNS_SIREN_2" };
        }

        #endregion
    }

    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new TValue();
            }

            return dict[key];
        }
    }
}