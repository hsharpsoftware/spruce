<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="itemcontainer">
        <div id="item">
            <h1><span class="workitem-id">#<%=Model.Id%></span>&nbsp;<%=Model.Title%></h1>
            <h2><%=Model.State%> | Assigned to <%=Model.AssignedTo%></h2>
            <p>
				<%=Model.Description%>
            </p>
        </div>
        <div id="favouritebar">
            <div id="date"><%=Model.CreatedDate.ToString()%>&nbsp;|&nbsp;Area: <%=Model.Area%></div>
            <div style="float:right;"><a href="javascript:;"><img src="/Spruce/Assets/Images/star_on.png" border="0" /></a></div>
        </div>
        <br style="clear:both"/>
        <div id="actionbar">
            <a href="/Spruce/Home/Edit/<%=Model.Id %>">Edit</a>&nbsp;|&nbsp;
            <a href="/Spruce/Home/Resolve/<%=Model.Id %>">Resolve</a>&nbsp;|&nbsp;
            <a href="/Spruce/Home/Close/<%=Model.Id %>">Close</a>
        </div>
    </div>

</asp:Content>


