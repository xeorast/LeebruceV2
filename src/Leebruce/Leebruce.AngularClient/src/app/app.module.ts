import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';

import { AuthenticationService } from './api/authentication.service'
import { ErrorInterceptorProvider } from './api/error.interceptor'

@NgModule( {
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    RouterModule.forRoot( [
      { path: '', component: HomeComponent },
      { path: 'counter', component: CounterComponent },
    ] )
  ],
  providers: [
    AuthenticationService,
    ErrorInterceptorProvider
  ],
  bootstrap: [AppComponent]
} )
export class AppModule { }
