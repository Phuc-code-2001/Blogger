
console.log("Profile Index JS");

const setupInfoBottom = () => {

    let headbox = $(".headbox");
    headbox.each((i, v) => {
        $(v).find(".tab-item").each((i, item) => {

            $(item).click(() => {
                let role = $(item).attr("role");
                $(v).find(".tab-item").each((i, o) => $(o).removeClass("active"));
                $(item).addClass("active");

                $("div[name='info_box']").each((k, v) => {
                    if($(v).attr("class") == role) $(v).show();
                    else $(v).hide();
                });
            });

        });

    })

}

$(document).ready(() => {
    $(".personal").hide();
    setupInfoBottom();

    // Hide some elements
    $(".btn_cancel_ok").hide();
    $(".personal_control_group").hide();

    $("#story_form").hide();
    
    // Setup update personal info
    $("#btn_update_personal").click(() => {
        $("#cancel_ok_personal").show();
        $("#btn_update_personal").hide();

        $("#update_info_form .text_editable").hide();
        $(".personal_control_group").show();
    });

    $("#btn_cancel_personal").click(() => {
        $("#btn_update_personal").show();
        $("#cancel_ok_personal").hide();

        $("#update_info_form .text_editable").show();
        $(".personal_control_group").hide();
    });

    $("#btn_update_story").click(() => {
        $("#btn_update_story").hide();
        $("#user_story").hide();
        $("#story_form").show();
        $("#cancel_ok_introduce").show();
        
        // Use jquery caret library
        $("#story_control").caretToEnd();
    });

    $("#btn_cancel_introduce").click(() => {
        $("#story_form").hide();
        $("#btn_update_story").show();
        $("#user_story").show();
    });

    $("#btn_update_image").on("click", function (e){
        let fileDialog = $('<input type="file" name="file" class="d-none">');
        fileDialog.click();
        fileDialog.on("change", function(e) {
            if($(this)[0].files) {
                $("#update_image_form").append(this);
                $("#update_image_form").submit();
            }
        });
        return false;
    });

    $("#btn_ok_personal").click(() => {
        $("#update_info_form").submit();
    });

    $("#btn_ok_introduce").click(() => {
        $("#story_form").submit();
    });
    
});