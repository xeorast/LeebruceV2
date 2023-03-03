import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { AnnouncementsComponent } from './announcements.component';
import { AnnouncementsClientModule } from '../api/announcements-client/announcements-client.module';


const routes: Routes = [
  { path: '', component: AnnouncementsComponent }
];

@NgModule( {
  declarations: [
    AnnouncementsComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild( routes ),
    AnnouncementsClientModule
  ]
} )
export class AnnouncementsModule { }
