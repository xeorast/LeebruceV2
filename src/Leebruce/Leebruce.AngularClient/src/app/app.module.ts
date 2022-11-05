import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';

import { CommonModule } from '@angular/common';
import { AuthenticationModule } from './api/authentication/authentication.module';
import { ErrorNotifierModule } from './api/error-notifier/error-notifier.module';
import { HttpErrorMapperModule } from './api/http-error-mapper/http-error-mapper.module';
import { ServerErrorHandlingModule } from './api/server-error-handling/server-error-handling.module';
import { NavMenuModule } from './nav-menu/nav-menu.module';

@NgModule( {
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    CommonModule,
    HttpClientModule,
    RouterModule.forRoot( [
      { path: '', loadChildren: () => import( './home/home.module' ).then( m => m.HomeModule ) },
      { path: 'counter', loadChildren: () => import( './counter/counter.module' ).then( m => m.CounterModule ) },
      { path: 'login', loadChildren: () => import( './login/login.module' ).then( m => m.LoginModule ) },
    ] ),
    NavMenuModule,
    AuthenticationModule,
    ServerErrorHandlingModule,
    HttpErrorMapperModule,
    ErrorNotifierModule
  ],
  bootstrap: [AppComponent]
} )
export class AppModule { }
