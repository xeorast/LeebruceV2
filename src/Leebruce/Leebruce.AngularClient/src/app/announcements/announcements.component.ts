import { Component, OnInit } from '@angular/core';
import { AnnouncementModel, AnnouncementsClientService } from '../api/announcements-client/announcements-client.service';

@Component( {
  selector: 'app-announcements',
  templateUrl: './announcements.component.html',
  styleUrls: ['./announcements.component.css']
} )
export class AnnouncementsComponent implements OnInit {

  constructor(
    private announcementService: AnnouncementsClientService
  ) { }

  public announcements?: AnnouncementModel[]

  ngOnInit(): void {
    this.announcementService.getAnnouncements().subscribe( {
      next: res => {
        this.announcements = res
      }
    } )
  }

}
