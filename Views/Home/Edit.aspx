<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<%using (new HtmlForm())
  { %>

	<input type="hidden" value="<%=Model.Id %>" />
    <div id="itemcontainer">
        <div id="item">
            <input type="text" name="title" value="<%=Model.Title%>" class="title" />
            <h2>#<%=Model.Id%>
			<!-- Assigned to -->
			Assigned to 
			<%=Html.DropDownBoxFromList("assignedto",(List<string>)ViewData["Users"],Model.AssignedTo)%>
            </h2>
            <textarea name="description" class="description"><%=Model.Description%></textarea>
        </div>
        <div id="actionbar" style="text-align:right;">
            <input type="submit" value="Save" class="button" />
            <input type="reset" value="Cancel" class="button" />
        </div>
    </div>
<%} %>
</asp:Content>


