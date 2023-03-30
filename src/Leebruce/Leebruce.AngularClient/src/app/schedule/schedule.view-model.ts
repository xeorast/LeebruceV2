import { ScheduleDayModel } from "../api/schedule-client/schedule-client.service"

export interface ScheduleViewModel {
    shownMonth: Date
    shownPageDays: Date[]
    today: Date
    daysMap?: { [dateValue: number]: ScheduleDayModel };

    selectedDate: Date
}
