import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GradesSubjectItemComponent } from './grades-subject-item.component';

describe('GradesSubjectItemComponent', () => {
  let component: GradesSubjectItemComponent;
  let fixture: ComponentFixture<GradesSubjectItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GradesSubjectItemComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GradesSubjectItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
