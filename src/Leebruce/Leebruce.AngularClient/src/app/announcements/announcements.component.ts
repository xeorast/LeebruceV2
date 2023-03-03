import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AnnouncementModel, AnnouncementsClientService } from '../api/announcements-client/announcements-client.service';
import { NotAuthenticatedError } from '../api/authentication/authentication.service';

@Component( {
  selector: 'app-announcements',
  templateUrl: './announcements.component.html',
  styleUrls: ['./announcements.component.css']
} )
export class AnnouncementsComponent implements OnInit {

  constructor(
    private router: Router,
    private announcementService: AnnouncementsClientService
  ) { }

  public announcements?: AnnouncementModel[]

  ngOnInit(): void {
    this.announcementService.getAnnouncements().subscribe( {
      next: res => {
        this.announcements = res
      },
      error: async error => {
        if ( error instanceof NotAuthenticatedError ) {
          let currentUrl = this.router.url
          await this.router.navigate( ['/login'], { queryParams: { redirect: currentUrl } } );
        }
      }
    } )
  }

}
