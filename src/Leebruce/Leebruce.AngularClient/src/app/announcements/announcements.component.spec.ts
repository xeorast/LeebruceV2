import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { AnnouncementsClientService, AnnouncementModel } from '../api/announcements-client/announcements-client.service';

import { AnnouncementsComponent } from './announcements.component';
import { AnnouncementsModule } from './announcements.module';

describe( 'AnnouncementsComponent', () => {
  let component: AnnouncementsComponent;
  let announcementsService: jasmine.SpyObj<AnnouncementsClientService>;
  let fixture: ComponentFixture<AnnouncementsComponent>;
  let announcementsSubject = new Subject<AnnouncementModel[]>();

  beforeEach( async () => {
    await TestBed.configureTestingModule( {
      imports: [AnnouncementsModule],
      providers: [
        {
          provide: AnnouncementsClientService,
          useValue: jasmine.createSpyObj( typeof AnnouncementsClientService, ['getAnnouncements'] )
        }
      ]
    } )
      .compileComponents();

    announcementsService = TestBed.inject( AnnouncementsClientService ) as jasmine.SpyObj<AnnouncementsClientService>
    announcementsService.getAnnouncements.and.returnValue( announcementsSubject.asObservable() )

    fixture = TestBed.createComponent( AnnouncementsComponent );
    component = fixture.componentInstance;
    fixture.detectChanges();
  } );

  it( 'should create', () => {
    expect( component ).toBeTruthy();
  } );
} );
