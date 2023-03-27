import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

@Component({
  selector: 'app-login-callback',
  template: '<div>Авторизация...</div>',
  styles: [],
})
export class LoginCallbackComponent implements OnInit {
  constructor(private readonly router: Router, private readonly authService: AuthService) {
  }

  async ngOnInit(): Promise<void> {
    await this.authService.signInCallback();
    await this.router.navigate(['']);
  }
}
