import { Component, OnInit } from '@angular/core';
import { AuthService } from "../services/auth.service";
import { User } from "oidc-client-ts";
import { TestService } from "../services/test.service";

@Component({
  selector: 'app-home',
  template: `
    <div>
      <h1>Hello World</h1>
      <div>{{ test }}</div>
      <ul>
        <li (click)="login()">Login</li>
        <li (click)="get()">Get</li>
      </ul>
    </div>
  `,
  styles: []
})
export class HomeComponent implements OnInit {
  private currentUser!: User;

  constructor(private authService: AuthService, private testService: TestService) {
  }

  ngOnInit(): void {
    this.authService.getUser().then(user => {
      this.currentUser = user;
    }).catch(error => console.log(error));
  }

  login(): void {
    console.log('redirecting');
    this.authService.signInRedirect().catch(error => console.log(error));
  }

  get(): Promise<void> {
    return this.testService.loadTest();
  }

  get test(): string {
    return this.testService.test;
  }
}
