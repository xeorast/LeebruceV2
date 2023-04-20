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
import { ScheduleClientModule } from './api/schedule-client/schedule-client.module';
import { StatusClientModule } from './api/status-client/status-client.module';
import { GradesClientModule } from './api/grades-client/grades-client.module';

@NgModule( {
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    CommonModule,
    HttpClientModule,
    RouterModule.forRoot( [
      { path: '', title: 'Home | Leebruce', loadChildren: () => import( './home/home.module' ).then( m => m.HomeModule ) },
      { path: 'counter', title: 'Clicker | Leebruce', loadChildren: () => import( './counter/counter.module' ).then( m => m.CounterModule ) },
      { path: 'login', title: 'Log in to Leebruce', loadChildren: () => import( './login/login.module' ).then( m => m.LoginModule ) },
      { path: 'announcements', title: 'Announcements | Leebruce', loadChildren: () => import( './announcements/announcements.module' ).then( m => m.AnnouncementsModule ) },
      { path: 'timetable', title: 'Timetable | Leebruce', loadChildren: () => import( './timetable/timetable.module' ).then( m => m.TimetableModule ) },
      { path: 'messages', title: 'Messages | Leebruce', loadChildren: () => import( './messages/messages.module' ).then( m => m.MessagesModule ) },
      { path: 'schedule', title: 'Schedule | Leebruce', loadChildren: () => import( './schedule/schedule.module' ).then( m => m.ScheduleModule ) },
      { path: 'grades', title: 'Grades | Leebruce', loadChildren: () => import( './grades/grades.module' ).then( m => m.GradesModule ) },
    ] ),
    NavMenuModule,
    AuthenticationModule,
    ServerErrorHandlingModule,
    HttpErrorMapperModule,
    ErrorNotifierModule,
    AnnouncementsClientModule,
    TimetableClientModule,
    MessagesClientModule,
    ScheduleClientModule,
    StatusClientModule,
    GradesClientModule,
  ],
  bootstrap: [AppComponent],
  providers: [
    { provide: FILESAVER, useFactory: getFileSaver }
  ]
} )
export class AppModule { }
