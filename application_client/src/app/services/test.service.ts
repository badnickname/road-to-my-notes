import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { AuthService } from "./auth.service";
import { environment } from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class TestService {

  constructor(private httpClient: HttpClient, private authService: AuthService) {
  }

  public test = "unloaded";

  public async loadTest(): Promise<void> {
    const user = await this.authService.getUser();
    const headers = new HttpHeaders({
      Accept: 'application/json',
      Authorization: 'Bearer ' + user.access_token
    });
    this.httpClient
      .get<string>(environment.applicationServer.url + '/test', { headers })
      .subscribe(x => this.test = x);
  }
}
