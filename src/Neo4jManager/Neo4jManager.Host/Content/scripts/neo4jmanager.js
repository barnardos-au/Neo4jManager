$(function () {

    $("#start-button").click(function (e, handler) {
        $.ajax({
            type: "POST",
            url: "/deployments/" + deploymentId
        });
        
        $.get("demo_test.asp", function(data, status){
            alert("Data: " + data + "\nStatus: " + status);
        });
    });
    
});


Deployment.Start = function(deploymentId){
	$.ajax({
		type: "POST",
		url: "/deployments/" + deploymentId
	});
};

