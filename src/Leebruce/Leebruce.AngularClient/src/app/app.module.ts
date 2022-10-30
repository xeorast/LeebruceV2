import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';

import { AuthenticationModule } from './api/authentication/authentication.module';
import { HttpErrorMapperModule } from './api/http-error-mapper/http-error-mapper.module';
import { NavMenuModule } from './nav-menu/nav-menu.module';

@NgModule( {
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    RouterModule.forRoot( [
      { path: '', loadChildren: () => import( './home/home.module' ).then( m => m.HomeModule ) },
      { path: 'counter', loadChildren: () => import( './counter/counter.module' ).then( m => m.CounterModule ) },
    ] ),
    NavMenuModule,
    AuthenticationModule,
    HttpErrorMapperModule,
  ],
  bootstrap: [AppComponent]
} )
export class AppModule { }
