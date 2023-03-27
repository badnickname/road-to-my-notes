import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './components/app.component';
import { AppRoutingModule } from './app-routing.module';
import { HomeComponent } from './components/home.component';
import { LoginCallbackComponent } from './components/login-callback.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    LoginCallbackComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
