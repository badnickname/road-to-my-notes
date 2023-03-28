import { Injectable } from '@angular/core';
import { User, UserManager } from "oidc-client-ts";
import { environment } from "src/environments/environment";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly userManager = new UserManager({
    authority: environment.identityServer.url,
    client_id: environment.identityClient.clientId,
    redirect_uri: `${environment.identityClient.redirectUrl}/login-callback`,
    post_logout_redirect_uri: environment.identityClient.redirectUrl,
    response_type: 'code',
    scope: environment.applicationServer.scope
  })

  public async getUser(): Promise<User> {
    const user = await this.userManager.getUser();
    if (!user) throw new Error('user is empty');
    return user;
  }

  public signInRedirect(): Promise<void> {
    return this.userManager.signinRedirect();
  }

  public async signInCallback(): Promise<void> {
    await this.userManager.signinCallback();
  }

  public async renewToken(): Promise<User> {
    const user = await this.userManager.signinSilent();
    if (!user) throw new Error('renew user is empty');
    return user;
  }
}
