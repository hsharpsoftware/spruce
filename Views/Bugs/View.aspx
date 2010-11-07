<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

	<div id="view-pagename" class="mid-text">#<%=Model.Id%></div>
    <div id="view-container">
		<div id="view-inner">
			<div class="title"><%=Model.Title%></div>
			<div class="extradetails dark-text">
				<%=Model.State%>, Assigned to <%=Model.AssignedTo%>
			</div>
			
			<div class="description">
				<p>
					<%=Model.Description.Replace("\n","<br/>")%>
				</p>
			</div>

			<div id="view-favouritebar">
				<div class="info dark-text">
					<%=Model.CreatedDate.ToString()%>, <%=Model.AreaName%>, <%=Model.IterationName%>
				</div>
				<div class="button dark-text"><a href="javascript:;"><img src="/Tfs/Spruce/Assets/Images/star_on.png" border="0" /></a></div>
			</div>
		</div>

        <br style="clear:both"/>
        <div id="view-actionbar" class="darkbg">
			<%=Html.ActionLink("Edit", "Edit", new { id = Model.Id })%>&nbsp;|&nbsp;
			<%=Html.ActionLink("Resolve", "Resolve", new { id = Model.Id })%>&nbsp;|&nbsp;
			<%=Html.ActionLink("Close", "Close", new { id = Model.Id })%>
        </div>
    </div>

</asp:Content>


