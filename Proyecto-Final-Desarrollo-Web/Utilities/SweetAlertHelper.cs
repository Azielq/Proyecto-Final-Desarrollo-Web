using System;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Helpers
{
    public class SweetAlertHelper
    {
        public static void ShowSweetAlert(Controller controller, string title, string message, SweetAlertMessageType messageType)
        {
            controller.TempData["SweetAlertTitle"] = title;
            controller.TempData["SweetAlertMessage"] = message;
            controller.TempData["SweetAlertType"] = messageType.ToString().ToLower();
        }

        public static void ShowSweetAlertWithRedirect(Controller controller, string title, string message, SweetAlertMessageType messageType, string redirectUrl)
        {
            controller.TempData["SweetAlertTitle"] = title;
            controller.TempData["SweetAlertMessage"] = message;
            controller.TempData["SweetAlertType"] = messageType.ToString().ToLower();
            controller.TempData["RedirectUrl"] = redirectUrl;
        }

        public static void ShowToast(Controller controller, string message, SweetAlertMessageType messageType)
        {
            controller.TempData["SweetAlertMessage"] = message;
            controller.TempData["SweetAlertType"] = messageType.ToString().ToLower();
            controller.TempData["SweetAlertIsToast"] = true;
        }
    }

    public enum SweetAlertMessageType
    {
        Success,
        Error,
        Warning,
        Info,
        Question
    }
}