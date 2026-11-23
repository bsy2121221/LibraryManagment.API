/*
(Re)creates every stored procedure. Safe to run any time: each procedure is dropped (if it exists)
and created again from its file in StoredProcedures\.

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).
Terminal:
  sqlcmd -S <server> -E -C -d LibraryManagement -i 01_CreateAllProcedures.sql -v DbProjectPath="<path to LibraryManagment.Database>"

When you add a new procedure file, add a DROP + :r pair for it here.
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

-- Procedures remember these settings at creation time. QUOTED_IDENTIFIER must be ON or any
-- procedure writing to a table with a filtered index (e.g. dbo.Bookings) fails at runtime.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Shared: other procedures queue messages through this one, so it is created first
DROP PROCEDURE IF EXISTS dbo.usp_Notification_EnqueueEvent;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_EnqueueEvent.sql
GO

-- Users
DROP PROCEDURE IF EXISTS dbo.usp_User_GetById;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_GetById.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_User_GetByMobile;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_GetByMobile.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_User_MobileExists;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_MobileExists.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_User_AnyAdminExists;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_AnyAdminExists.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_User_Create;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_Create.sql
GO

-- Refresh tokens
DROP PROCEDURE IF EXISTS dbo.usp_RefreshToken_GetByHash;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_RefreshToken_GetByHash.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_RefreshToken_Create;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_RefreshToken_Create.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_RefreshToken_Revoke;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_RefreshToken_Revoke.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_RefreshToken_RevokeAllForUser;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_RefreshToken_RevokeAllForUser.sql
GO

-- Sections
DROP PROCEDURE IF EXISTS dbo.usp_Section_GetAll;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Section_GetAll.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Section_GetById;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Section_GetById.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Section_Create;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Section_Create.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Section_Update;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Section_Update.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Section_Delete;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Section_Delete.sql
GO

-- Seats
DROP PROCEDURE IF EXISTS dbo.usp_Seat_GetBySection;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Seat_GetBySection.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Seat_SaveLayout;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Seat_SaveLayout.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_SeatMap_Get;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_SeatMap_Get.sql
GO

-- Settings
DROP PROCEDURE IF EXISTS dbo.usp_Setting_GetAll;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Setting_GetAll.sql
GO

-- Bookings (need the views from 02_CreateAllViews.sql)
DROP PROCEDURE IF EXISTS dbo.usp_Booking_GetById;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_GetById.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_GetLiveByUser;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_GetLiveByUser.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_Hold;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_Hold.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_CancelHold;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_CancelHold.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_ExpireHolds;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_ExpireHolds.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_ConfirmPayment;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_ConfirmPayment.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_GetAllForAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_GetAllForAdmin.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_Release;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_Release.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_MoveSeat;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_MoveSeat.sql
GO

-- Payments
DROP PROCEDURE IF EXISTS dbo.usp_Payment_Create;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Payment_Create.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Payment_GetByOrderId;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Payment_GetByOrderId.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Payment_MarkFailed;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Payment_MarkFailed.sql
GO

-- Step 3: student management and settings
DROP PROCEDURE IF EXISTS dbo.usp_User_GetStudentsForAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_GetStudentsForAdmin.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_User_UpdateProfile;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_UpdateProfile.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_User_UpdatePassword;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_UpdatePassword.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_SeatChangeHistory_GetByUser;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_SeatChangeHistory_GetByUser.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Payment_GetByUser;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Payment_GetByUser.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Setting_UpdateMany;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Setting_UpdateMany.sql
GO

-- Step 4: notifications (queue), fees and billing
DROP PROCEDURE IF EXISTS dbo.usp_Notification_EnqueueFeeReminders;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_EnqueueFeeReminders.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_ClaimBatch;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_ClaimBatch.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_MarkSent;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_MarkSent.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_MarkFailed;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_MarkFailed.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_GetForAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_GetForAdmin.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_GetSummary;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_GetSummary.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_Retry;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_Retry.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Notification_Cancel;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Notification_Cancel.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_NotificationEvent_GetAll;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_NotificationEvent_GetAll.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_NotificationEvent_SaveChannel;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_NotificationEvent_SaveChannel.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_NotificationRule_GetAll;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_NotificationRule_GetAll.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_NotificationRule_Save;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_NotificationRule_Save.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_NotificationRule_Delete;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_NotificationRule_Delete.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Billing_Run;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Billing_Run.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_FeeInvoice_GetById;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_FeeInvoice_GetById.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_FeeInvoice_GetByUser;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_FeeInvoice_GetByUser.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_FeeInvoice_GetForAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_FeeInvoice_GetForAdmin.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_FeeInvoice_MarkPaid;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_FeeInvoice_MarkPaid.sql
GO

-- Walk-in students: admin registers, assigns a seat, adds fees
DROP PROCEDURE IF EXISTS dbo.usp_User_CreateByAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_User_CreateByAdmin.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_Booking_AssignByAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_Booking_AssignByAdmin.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_FeeInvoice_CreateByAdmin;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_FeeInvoice_CreateByAdmin.sql
GO

-- Media library and home page builder
DROP PROCEDURE IF EXISTS dbo.usp_MediaAsset_Create;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_MediaAsset_Create.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_MediaAsset_GetAll;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_MediaAsset_GetAll.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_MediaAsset_Delete;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_MediaAsset_Delete.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_HomePageLayout_Save;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_HomePageLayout_Save.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_HomePageLayout_Publish;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_HomePageLayout_Publish.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_HomePageLayout_GetPublished;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_HomePageLayout_GetPublished.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_HomePageLayout_GetById;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_HomePageLayout_GetById.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_HomePageLayout_GetHistory;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_HomePageLayout_GetHistory.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_SiteNotice_GetAll;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_SiteNotice_GetAll.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_SiteNotice_GetCurrent;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_SiteNotice_GetCurrent.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_SiteNotice_Save;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_SiteNotice_Save.sql
GO
DROP PROCEDURE IF EXISTS dbo.usp_SiteNotice_Delete;
GO
:r $(DbProjectPath)\StoredProcedures\dbo.usp_SiteNotice_Delete.sql
GO
