using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    Timeout
}

public static class Authenticator
{
    public static AuthState state { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (state == AuthState.Authenticating) return state;

        await SignInAnonymouslyAsync(maxTries);

        return state;
    }

    static async Task SignInAnonymouslyAsync(int maxTries = 5)
    {
        state = AuthState.Authenticating;

        int tries = 0;

        while (state == AuthState.Authenticating && tries < maxTries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    state = AuthState.Authenticated;
                    return;
                }
            }
            catch (AuthenticationException e)
            {
                Debug.LogException(e);
                state = AuthState.Error;
                return;
            }
            catch (RequestFailedException e)
            {
                Debug.LogException(e);
                state = AuthState.Error;
                return;
            }

            tries++;
            await Task.Delay(1000);
        }

        state = AuthState.Timeout;
    }


}
