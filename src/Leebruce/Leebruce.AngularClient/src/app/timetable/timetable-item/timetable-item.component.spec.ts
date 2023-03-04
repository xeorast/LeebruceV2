import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LessonModel, LessonTimeModel } from 'src/app/api/timetable-client/timetable-client.service';

import { TimetableItemComponent } from './timetable-item.component';

describe( 'TimetableItemComponent', () => {
  let component: TimetableItemComponent;
  let fixture: ComponentFixture<TimetableItemComponent>;

  beforeEach( async () => {
    await TestBed.configureTestingModule( {
      declarations: [TimetableItemComponent]
    } )
      .compileComponents();

    fixture = TestBed.createComponent( TimetableItemComponent );
    component = fixture.componentInstance;
    component.lessonModel = <LessonModel>{
      time: <LessonTimeModel>{},
    }
    fixture.detectChanges();
  } );

  it( 'should create', () => {
    expect( component ).toBeTruthy();
  } );
} );
