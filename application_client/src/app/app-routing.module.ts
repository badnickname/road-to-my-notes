import { NgModule } from '@angular/core';
import { RouterModule, Routes } from "@angular/router";
import { HomeComponent } from "./components/home.component";
import { LoginCallbackComponent } from "./components/login-callback.component";

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login-callback', component: LoginCallbackComponent }
]

@NgModule({
  declarations: [],
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports: [RouterModule],
})

export class AppRoutingModule {
}
