using System;
using System.Collections;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-9900)]
    public sealed class KickLuckyCubeMirraCloudService : MonoBehaviour
    {
        [SerializeField] private bool autoLoginAsGuest = true;
        [SerializeField] private bool logConnectionState = true;

        public static KickLuckyCubeMirraCloudService Instance { get; private set; }

        public IMirraCloudSdk Sdk { get; private set; }
        public bool IsConfigured { get; private set; }
        public bool IsReady => Sdk != null && Sdk.IsInitialized;
        public bool IsAuthenticated => IsReady && Sdk.Authentication.IsAuth;
        public string LastError { get; private set; }

        public event Action CloudReady;
        public event Action<GetAuthDataDto> Authenticated;
        public event Action PlayerSessionReady;
        public event Action<string> ConnectionFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var configuration = Resources.Load<Configuration>("Configuration");
            IsConfigured = HasRuntimeConfiguration(configuration);
            if (!IsConfigured)
            {
                if (logConnectionState)
                {
                    Debug.Log(
                        "Kick Lucky Cube: Mirra Cloud is installed but not connected. " +
                        "Open Tools > Mirra Cloud > Manager and select a project, branch, and API token.");
                }

                return;
            }

            Sdk = MirraCloudSDK.Create();
            Sdk.Initialize();
            Sdk.Authentication.OnLogin += HandleLogin;
            Sdk.Authentication.OnSessionExpired += HandleSessionExpired;
            CloudReady?.Invoke();
        }

        private IEnumerator Start()
        {
            if (!IsReady)
            {
                yield break;
            }

            var restoreOperation = Sdk.Authentication.InitializeAsync();
            yield return restoreOperation;
            if (!restoreOperation.Result.IsSuccess)
            {
                ReportFailure("session restore", restoreOperation.Result.Error?.Message);
                yield break;
            }

            if (Sdk.Authentication.IsAuth || !autoLoginAsGuest)
            {
                if (Sdk.Authentication.IsAuth)
                {
                    var accountOperation = Sdk.PlayerAccount.GetAccountAsync();
                    yield return accountOperation;
                    if (!accountOperation.Result.IsSuccess)
                    {
                        ReportFailure("player account restore", accountOperation.Result.Error?.Message);
                        yield break;
                    }

                    LogPlayerSessionReady("restored");
                }

                yield break;
            }

            var loginOperation = Sdk.Authentication.LoginGuestAsync();
            yield return loginOperation;
            if (!loginOperation.Result.IsSuccess)
            {
                ReportFailure("guest login", loginOperation.Result.Error?.Message);
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (Sdk != null)
            {
                Sdk.Authentication.OnLogin -= HandleLogin;
                Sdk.Authentication.OnSessionExpired -= HandleSessionExpired;
                Sdk.Dispose();
                Sdk = null;
            }

            Instance = null;
        }

        private void HandleLogin(GetAuthDataDto authData)
        {
            LastError = null;
            if (logConnectionState)
            {
                Debug.Log("Kick Lucky Cube: Mirra Cloud player session is ready.");
            }

            Authenticated?.Invoke(authData);
            PlayerSessionReady?.Invoke();
        }

        private void LogPlayerSessionReady(string source)
        {
            LastError = null;
            if (logConnectionState)
            {
                Debug.Log($"Kick Lucky Cube: Mirra Cloud player session is ready ({source}).");
            }

            PlayerSessionReady?.Invoke();
        }

        private void HandleSessionExpired()
        {
            ReportFailure("session", "The Mirra Cloud player session expired.");
        }

        private void ReportFailure(string stage, string message)
        {
            LastError = string.IsNullOrWhiteSpace(message)
                ? $"Mirra Cloud {stage} failed."
                : $"Mirra Cloud {stage} failed: {message}";
            Debug.LogWarning($"Kick Lucky Cube: {LastError}");
            ConnectionFailed?.Invoke(LastError);
        }

        private static bool HasRuntimeConfiguration(Configuration configuration)
        {
            return configuration != null
                && !string.IsNullOrWhiteSpace(configuration.ProjectId)
                && !string.IsNullOrWhiteSpace(configuration.BranchId)
                && !string.IsNullOrWhiteSpace(configuration.Token);
        }
    }
}
