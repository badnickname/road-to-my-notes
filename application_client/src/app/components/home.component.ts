import { Component, OnInit } from '@angular/core';
import { AuthService } from "../services/auth.service";
import { User } from "oidc-client-ts";

@Component({
  selector: 'app-home',
  template: `
    <div>
      <h1>Hello World</h1>
      <ul>
        <li (click)="login()">Login</li>
      </ul>
    </div>
  `,
  styles: []
})
export class HomeComponent implements OnInit {
  private currentUser!: User;

  constructor(private authService: AuthService) {
  }

  ngOnInit(): void {
    this.authService.getUser().then(user => {
      this.currentUser = user;
    }).catch(error => console.log(error));
  }

  login() {
    console.log('redirecting');
    this.authService.signInRedirect().catch(error => console.log(error));
  }
}
