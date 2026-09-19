package com.autofnhk.android

import android.app.Activity
import android.os.Bundle
import android.graphics.Color
import android.view.Gravity
import android.widget.*
import android.content.Context

class MainActivity : Activity() {
    private lateinit var status: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val root = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(24, 24, 24, 24)
            setBackgroundColor(Color.rgb(11, 11, 18))
        }

        val title = TextView(this).apply {
            text = "AutoFnhk"
            textSize = 28f
            setTextColor(Color.WHITE)
            setPadding(0, 0, 0, 16)
        }
        root.addView(title)

        val tabs = LinearLayout(this).apply {
            orientation = LinearLayout.HORIZONTAL
        }
        val aiButton = Button(this).apply { text = "AI" }
        val settingsButton = Button(this).apply { text = "Settings" }
        val diagnosticsButton = Button(this).apply { text = "Diagnostics" }
        tabs.addView(aiButton, LinearLayout.LayoutParams(0, -2, 1f))
        tabs.addView(settingsButton, LinearLayout.LayoutParams(0, -2, 1f))
        tabs.addView(diagnosticsButton, LinearLayout.LayoutParams(0, -2, 1f))
        root.addView(tabs)

        val content = FrameLayout(this)
        root.addView(content, LinearLayout.LayoutParams(-1, 0, 1f))

        status = TextView(this).apply {
            text = "Press the AI button to enter a plan."
            textSize = 15f
            setTextColor(Color.LTGRAY)
            setPadding(0, 16, 0, 0)
        }
        root.addView(status)

        fun showAi() {
            content.removeAllViews()
            val box = LinearLayout(this).apply {
                orientation = LinearLayout.VERTICAL
                setPadding(0, 20, 0, 0)
            }
            val prompt = EditText(this).apply {
                hint = "Enter an AI plan..."
                setTextColor(Color.WHITE)
                setHintTextColor(Color.GRAY)
                minLines = 4
                gravity = Gravity.TOP
            }
            val begin = Button(this).apply { text = "Begin AI plan" }
            begin.setOnClickListener {
                status.text = "AI plan ready. Android control execution is not connected."
            }
            box.addView(prompt, LinearLayout.LayoutParams(-1, 0, 1f))
            box.addView(begin)
            content.addView(box)
            status.text = "Type an instruction, then press Begin."
        }

        fun showSettings() {
            content.removeAllViews()
            val box = LinearLayout(this).apply {
                orientation = LinearLayout.VERTICAL
                setPadding(0, 20, 0, 0)
            }
            addSwitch(box, "Aim Lock", true)
            addSwitch(box, "Auto Smoothing", true)
            addSwitch(box, "Diagnostics / Event Logging", true)
            addSwitch(box, "Learning / Performance Feedback", true)
            addSwitch(box, "Human-like Variation", true)

            val skill = Spinner(this)
            skill.adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item,
                (1..5).map { "Skill level $it" })
            box.addView(skill)
            content.addView(box)
            status.text = "Settings are saved for this session."
        }

        fun showDiagnostics() {
            content.removeAllViews()
            val log = TextView(this).apply {
                text = "Diagnostics\n\nNo events recorded yet."
                textSize = 15f
                setTextColor(Color.LTGRAY)
                setPadding(0, 20, 0, 0)
            }
            content.addView(log)
            status.text = "Diagnostics ready."
        }

        aiButton.setOnClickListener { showAi() }
        settingsButton.setOnClickListener { showSettings() }
        diagnosticsButton.setOnClickListener { showDiagnostics() }

        setContentView(root)
        showAi()
    }

    private fun addSwitch(parent: LinearLayout, label: String, checked: Boolean) {
        parent.addView(Switch(this).apply {
            text = label
            isChecked = checked
            setTextColor(Color.WHITE)
            setPadding(0, 8, 0, 8)
        })
    }
}
