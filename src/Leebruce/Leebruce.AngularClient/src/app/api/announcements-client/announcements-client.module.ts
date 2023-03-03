import { NgModule } from '@angular/core';
import { AuthenticationModule } from '../authentication/authentication.module';

import { AnnouncementsClientService } from './announcements-client.service';



@NgModule( {
  declarations: [],
  imports: [
  ],
  providers: [
    AnnouncementsClientService
  ]
} )
export class AnnouncementsClientModule { }
