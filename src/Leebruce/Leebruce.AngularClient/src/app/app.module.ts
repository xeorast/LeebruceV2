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
import { AnnouncementsClientModule } from './api/announcements-client/announcements-client.module';
import { TimetableClientModule } from './api/timetable-client/timetable-client.module';
import { MessagesClientModule } from './api/messages-client/messages-client.module';
import { getFileSaver, FILESAVER } from './api/fileSaver';

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
      { path: 'announcements', loadChildren: () => import( './announcements/announcements.module' ).then( m => m.AnnouncementsModule ) },
      { path: 'timetable', loadChildren: () => import('./timetable/timetable.module').then(m => m.TimetableModule) },
    ] ),
    NavMenuModule,
    AuthenticationModule,
    ServerErrorHandlingModule,
    HttpErrorMapperModule,
    ErrorNotifierModule,
    AnnouncementsClientModule,
    TimetableClientModule,
    MessagesClientModule,
  ],
  bootstrap: [AppComponent],
  providers: [
    { provide: FILESAVER, useFactory: getFileSaver }
  ]
} )
export class AppModule { }
